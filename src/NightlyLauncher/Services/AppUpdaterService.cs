using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class AppUpdaterService(
    HttpClient httpClient,
    IConfigService configService,
    IDatabaseService databaseService,
    IPathService pathService,
    IStringLocalizer<AppUpdaterService> localizer) : IAppUpdaterService {
    public async Task<bool> NeedsUpdateAsync(InstalledApp app, AppManifest manifest,
        CancellationToken cancellationToken = default) {
        var option = app.Manifest.InstallOptions.First(o => o.Guid == manifest.SelectedInstallOptionGuid);
        return app.LocalVersion != await GetLatestVersionAsync(option, cancellationToken);
    }

    public async Task UpdateAsync(InstalledApp app, AppManifest manifest, IProgress<double>? downloadProgress,
        CancellationToken cancellationToken = default) {
        Directory.CreateDirectory(configService.Config.AppPath);
        Directory.CreateDirectory(configService.Config.DownloadPath);

        var option = app.Manifest.InstallOptions.First(o => o.Guid == manifest.SelectedInstallOptionGuid);

        var archivePath = Path.Combine(pathService.DownloadPath, option.DownloadUri.LocalPath);
        var latestVersion = await GetLatestVersionAsync(option, cancellationToken);

        try {
            await DownloadFileAsync(
                option.DownloadUri, pathService.DownloadPath, option.DownloadUri.LocalPath, downloadProgress,
                cancellationToken);

            var installPath = pathService.GetInstallPath(app);

            Directory.CreateDirectory(installPath);

            await ZipFile.ExtractToDirectoryAsync(
                archivePath, installPath, true, cancellationToken);

            if (manifest.Portable && option.PortablePath is not null) {
                if (option.PortablePath.PortablePathType == PortablePathType.File)
                    await File.Create(Path.Combine(installPath, option.PortablePath.Path)).DisposeAsync();
                else
                    Directory.CreateDirectory(Path.Combine(installPath, option.PortablePath.Path));
            }

            await databaseService.UpdateAppAsync(
                new InstalledApp(manifest, latestVersion, latestVersion, DateTime.UtcNow), cancellationToken);
        } finally {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
        }
    }

    private async Task<string> GetLatestVersionAsync(InstallOption option, CancellationToken cancellationToken) {
        var result = await httpClient.GetFromJsonAsync<CommitResponse>(option.LatestCommitUri, cancellationToken);
        return result?.Sha ?? throw new InvalidOperationException(localizer["EmptyCommitHash"]);
    }

    private async Task DownloadFileAsync(Uri uri, string downloadPath, string fileName, IProgress<double>? progress,
        CancellationToken cancellationToken) {
        Directory.CreateDirectory(downloadPath);

        var fullPath = Path.Combine(downloadPath, fileName);
        var tempPath = fullPath + ".tmp";

        try {
            using var response =
                await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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

            File.Move(tempPath, fullPath, true);
        } catch {
            try {
                File.Delete(tempPath);
            } catch {
                /* ignore */
            }

            throw;
        }
    }

    private sealed record CommitResponse(
        [property: JsonPropertyName("sha")] string? Sha);
}
