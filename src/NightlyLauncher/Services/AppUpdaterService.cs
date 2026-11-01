using System;
using System.Buffers;
using System.Diagnostics;
using System.Formats.Tar;
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
    IDatabaseService databaseService,
    IPathService pathService,
    IStringLocalizer<AppUpdaterService> localizer) : IAppUpdaterService {
    public void DeleteAppData(InstalledApp app) {
        try {
            Directory.Delete(pathService.GetInstallPath(app), true);
        } catch (DirectoryNotFoundException) {
            /* ignore */
        }
    }

    public async Task UpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        progress.Report(new ProgressReport(-1, localizer["Updating"]));
        var option = databaseService.FindInstallOption(app);

        var supportedFileTypes = new[] { ".zip", ".tar", ".tar.gz" };
        if (!supportedFileTypes.Any(fileType =>
                option.DownloadUri.LocalPath.EndsWith(fileType, StringComparison.OrdinalIgnoreCase)))
            throw new NotSupportedException(localizer["UnsupportedArchiveFileType"]);

        var archivePath = Path.Combine(pathService.DownloadPath, Path.GetFileName(option.DownloadUri.LocalPath));

        try {
            await DownloadFileAsync(
                option.DownloadUri, progress,
                cancellationToken);

            progress.Report(new ProgressReport(-1, localizer["Updating"]));

            var installPath = pathService.GetInstallPath(app);
            Directory.CreateDirectory(installPath);

            await ExtractArchive(archivePath, installPath, cancellationToken);

            await ApplyPortableAsync(app, cancellationToken);

            var latestVersion = await GetLatestVersionAsync(option, cancellationToken);

            await databaseService.UpdateAppAsync(
                new InstalledApp(app.Manifest, app.SelectedInstallOptionGuid, latestVersion, latestVersion,
                    DateTime.UtcNow), cancellationToken);

            progress.Report(new ProgressReport(100, localizer["UpdateComplete"]));
        } finally {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
        }
    }

    public async Task<bool> NeedsUpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        progress.Report(new ProgressReport(-1, localizer["CheckingForUpdates"]));

        if (app.LocalVersion != app.LatestVersion) {
            progress.Report(new ProgressReport(-1, localizer["UpdateFound"]));
            return true;
        }

        var option = databaseService.FindInstallOption(app);
        var latestVersion = await GetLatestVersionAsync(option, cancellationToken);

        if (app.LatestVersion != latestVersion)
            await databaseService.UpdateAppAsync(app with { LatestVersion = latestVersion }, cancellationToken);

        if (app.LocalVersion != latestVersion) {
            progress.Report(new ProgressReport(-1, localizer["UpdateFound"]));
            return true;
        }

        progress.Report(new ProgressReport(-1, localizer["NoUpdateFound"]));
        return false;
    }

    public async Task ApplyPortableAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        var portablePath = databaseService.FindInstallOption(app).PortablePath;

        if (portablePath is null) return;

        var path = Path.Combine(pathService.GetInstallPath(app), portablePath.Path);
        var disabledPath = path + ".disabled";
        var (source, destination) = app.Manifest.Portable ? (disabledPath, path) : (path, disabledPath);

        if (portablePath.PortablePathType is PortablePathType.Folder) {
            if (Directory.Exists(source))
                Directory.Move(source, destination);
            else if (app.Manifest.Portable)
                Directory.CreateDirectory(path);
        } else {
            if (File.Exists(source))
                File.Move(source, destination);
            else if (app.Manifest.Portable)
                await File.WriteAllBytesAsync(path, [], cancellationToken);
        }
    }

    private async Task ExtractArchive(string archivePath, string destinationPath, CancellationToken cancellationToken) {
        if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            await ZipFile.ExtractToDirectoryAsync(archivePath, destinationPath, true, cancellationToken);
        else if (archivePath.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
            await TarFile.ExtractToDirectoryAsync(archivePath, destinationPath, true, cancellationToken);
        else if (archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)) {
            await using var fileStream = File.OpenRead(archivePath);
            await using var gZipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gZipStream, destinationPath, true, cancellationToken);
        } else
            throw new NotSupportedException(localizer["UnsupportedArchiveFileType"]);
    }

    private async Task<string> GetLatestVersionAsync(InstallOption option, CancellationToken cancellationToken) {
        var result = await httpClient.GetFromJsonAsync<CommitResponse>(option.LatestCommitUri, cancellationToken);
        return result?.Sha ?? throw new InvalidOperationException(localizer["EmptyCommitHash"]);
    }

    private async Task DownloadFileAsync(Uri uri, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken) {
        var downloadPath = pathService.DownloadPath;
        Directory.CreateDirectory(downloadPath);

        var fullPath = Path.Combine(downloadPath, Path.GetFileName(uri.LocalPath));
        var tempPath = fullPath + ".tmp";

        try {
            using var response =
                await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;

            if (contentLength is null or 0)
                progress.Report(new ProgressReport(-1, localizer["DownloadingUpdate"]));

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using (var target = new FileStream(tempPath,
                             new FileStreamOptions {
                                 Mode = FileMode.Create,
                                 Access = FileAccess.Write,
                                 Share = FileShare.None,
                                 BufferSize = 0,
                                 Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                                 PreallocationSize = contentLength ?? 0
                             })) {
                var buffer = ArrayPool<byte>.Shared.Rent(256 * 1024);

                try {
                    long totalRead = 0;
                    var lastReport = Stopwatch.GetTimestamp();
                    int read;

                    while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0) {
                        await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        totalRead += read;

                        if (contentLength is > 0 &&
                            Stopwatch.GetElapsedTime(lastReport) > TimeSpan.FromMilliseconds(500)) {
                            progress.Report(new ProgressReport(
                                Math.Clamp((int)(totalRead * 100 / contentLength.Value), 0, 100),
                                localizer["DownloadingUpdate"]));
                            lastReport = Stopwatch.GetTimestamp();
                        }
                    }

                    if (contentLength.HasValue && totalRead != contentLength.Value)
                        throw new IOException(localizer["DownloadIncomplete"]);

                    progress.Report(new ProgressReport(100, localizer["DownloadingUpdate"]));
                } finally {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            File.Move(tempPath, fullPath, true);
            progress.Report(new ProgressReport(100, localizer["DownloadComplete"]));
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
