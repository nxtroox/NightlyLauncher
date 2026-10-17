using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class AppUpdaterService(HttpClient httpClient, IConfigService configService, IDatabaseService databaseService, IPathService pathService, IStringLocalizer<AppUpdaterService> localizer) : IAppUpdaterService {
    private async Task<string> GetLatestVersionAsync(InstallOperation operation, CancellationToken cancellationToken) {
        var result = await httpClient.GetFromJsonAsync<CommitResponse>(operation.LatestCommitUri, cancellationToken);
        return result?.Sha ?? throw new InvalidOperationException(localizer["EmptyCommitHash"]);
    }

    private sealed record CommitResponse(
        [property: JsonPropertyName("sha")] string? Sha);

    private async Task DownloadFileAsync(Uri uri, string downloadPath, string fileName, IProgress<double>? progress, CancellationToken cancellationToken) {
        Directory.CreateDirectory(downloadPath);

        var fullPath = Path.Combine(downloadPath, fileName);
        var tempPath = fullPath + ".tmp";

        try {
            using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;

            if (contentLength is null or 0)
                progress?.Report(-1);

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var target = new FileStream(tempPath,
                new FileStreamOptions {
                    Mode = FileMode.Create,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    BufferSize = 0,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                    PreallocationSize = contentLength ?? 0
                });

            var buffer = ArrayPool<byte>.Shared.Rent(256 * 1024);

            try {
                long totalRead = 0;
                var lastReport = Stopwatch.GetTimestamp();
                int read;

                while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0) {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    totalRead += read;

                    if (contentLength is > 0 && Stopwatch.GetElapsedTime(lastReport) > TimeSpan.FromMilliseconds(250)) {
                        progress?.Report(Math.Clamp((double)totalRead / contentLength.Value * 100.0, 0, 100));
                        lastReport = Stopwatch.GetTimestamp();
                    }
                }

                if (contentLength.HasValue && totalRead != contentLength.Value)
                    throw new IOException(localizer["DownloadIncomplete"]);

                progress?.Report(100);
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            File.Move(tempPath, fullPath, overwrite: true);
        } catch {
            try { File.Delete(tempPath); } catch { /* ignore */ }
            throw;
        }
    }

    public async Task<bool> NeedsUpdateAsync(InstallOperation operation, AppManifest manifest, CancellationToken cancellationToken = default) {
        var localVersion = await databaseService.GetLocalVersionAsync(manifest, cancellationToken);
        var newVersion = await GetLatestVersionAsync(operation, cancellationToken);

        return localVersion != newVersion;
    }

    public async Task UpdateAsync(InstallOperation operation, AppManifest manifest, IProgress<double>? downloadProgress, CancellationToken cancellationToken = default) {
        Directory.CreateDirectory(configService.Config.AppPath);
        Directory.CreateDirectory(configService.Config.DownloadPath);

        var archivePath = Path.Combine(pathService.DownloadPath, operation.DownloadUri.LocalPath);
        var latestVersion = await GetLatestVersionAsync(operation, cancellationToken);

        try {
            await DownloadFileAsync(
                operation.DownloadUri, pathService.DownloadPath, operation.DownloadUri.LocalPath, downloadProgress, cancellationToken);

            Directory.CreateDirectory(operation.InstallPath);

            await ZipFile.ExtractToDirectoryAsync(
                archivePath, operation.InstallPath, overwriteFiles: true, cancellationToken);

            if (manifest.Portable && operation.PortablePath is not null) {
                if (operation.PortablePath.PortablePathType == PortablePathType.File)
                    await File.Create(Path.Combine(operation.InstallPath, operation.PortablePath.Path)).DisposeAsync();
                else
                    Directory.CreateDirectory(Path.Combine(operation.InstallPath, operation.PortablePath.Path));
            }

            await databaseService.UpdateAppAsync(
                new InstalledApp(manifest, latestVersion, latestVersion, DateTime.UtcNow), cancellationToken);
        } finally {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
        }
    }
}
