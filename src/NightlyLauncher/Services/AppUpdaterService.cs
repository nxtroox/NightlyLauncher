using System.Buffers;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Core.Models;
using NightlyLauncher.Models;
using NightlyLauncher.Models.UI;
using NightlyLauncher.Serialization;

namespace NightlyLauncher.Services;

internal sealed class AppUpdaterService(
    HttpClient httpClient,
    IDatabaseService databaseService,
    IPathService pathService,
    IStringLocalizer<AppUpdaterService> localizer) : IAppUpdaterService {
    public void DeleteAppData(InstalledApp app) {
        try {
            var icons = Directory.EnumerateFiles(pathService.IconPath, app.Manifest.Guid + "*");
            foreach (var icon in icons)
                File.Delete(icon);

            Directory.Delete(pathService.GetAppPath(app), true);
        } catch (DirectoryNotFoundException) {
            /* ignore */
        }
    }

    public async Task UpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        progress.Report(new ProgressReport(-1, localizer["UpdatingProgressReportStatusText"]));
        var option = databaseService.FindInstallOption(app);

        var supportedFileTypes = new[] { ".zip", ".tar", ".tar.gz" };
        if (!supportedFileTypes.Any(fileType =>
                option.DownloadUri.LocalPath.EndsWith(fileType, StringComparison.OrdinalIgnoreCase)))
            throw new NotSupportedException(localizer["UnsupportedArchiveFileTypeExceptionMessage"]);

        var archivePath = Path.Combine(pathService.DownloadPath, Path.GetFileName(option.DownloadUri.LocalPath));

        try {
            await DownloadFileAsync(option.DownloadUri, pathService.DownloadPath, progress, cancellationToken);

            if (app.Manifest.IconUri is not null) {
                var fileName = app.Manifest.Guid + Path.GetExtension(app.Manifest.IconUri.LocalPath);
                var iconPath = Path.Combine(pathService.IconPath, fileName);

                if (!Path.Exists(iconPath))
                    await DownloadFileAsync(app.Manifest.IconUri, pathService.IconPath, progress, cancellationToken,
                        fileName);
            }

            progress.Report(new ProgressReport(-1, localizer["UpdatingProgressReportStatusText"]));

            var installPath = pathService.GetAppPath(app);
            Directory.CreateDirectory(installPath);

            await ExtractArchive(archivePath, installPath, cancellationToken);

            await ApplyPortableAsync(app, cancellationToken);

            var latestVersion = await GetLatestVersionAsync(app.Manifest.LatestCommitApiUri, cancellationToken);

            await databaseService.UpdateAppAsync(
                app with { LastUpdatedOn = DateTime.UtcNow, LocalVersion = latestVersion }, cancellationToken);

            progress.Report(new ProgressReport(100, localizer["UpdateCompleteProgressReportStatusText"]));
        } finally {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
        }
    }

    public async Task<bool> NeedsUpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        progress.Report(new ProgressReport(-1, localizer["CheckingForUpdatesProgressReportStatusText"]));

        var latestVersion = await GetLatestVersionAsync(app.Manifest.LatestCommitApiUri, cancellationToken);

        if (app.LocalVersion != latestVersion) {
            progress.Report(new ProgressReport(-1, localizer["UpdateFoundProgressReportStatusText"]));
            return true;
        }

        progress.Report(new ProgressReport(-1, localizer["NoUpdateFoundProgressReportStatusText"]));
        return false;
    }

    public async Task ApplyPortableAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        var portablePath = databaseService.FindInstallOption(app).PortablePath;

        if (portablePath is null) return;
        var portableOptions = portablePath.Value;

        var path = Path.Combine(pathService.GetAppPath(app), portableOptions.Path);
        var disabledPath = path + ".disabled";
        var (source, destination) = app.Options.Portable ? (disabledPath, path) : (path, disabledPath);

        if (portableOptions.PortableType is PortableType.Folder) {
            if (Directory.Exists(source))
                Directory.Move(source, destination);
            else if (app.Options.Portable)
                Directory.CreateDirectory(path);
        } else {
            if (File.Exists(source))
                File.Move(source, destination);
            else if (app.Options.Portable)
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
            throw new NotSupportedException(localizer["UnsupportedArchiveFileTypeExceptionMessage"]);
    }

    private async Task<string> GetLatestVersionAsync(Uri latestCommitApiUri, CancellationToken cancellationToken) {
        var result = await httpClient.GetFromJsonAsync<CommitResponse>(latestCommitApiUri,
            AppJsonContext.Default.CommitResponse, cancellationToken);
        return result?.Sha ?? throw new InvalidOperationException(localizer["EmptyCommitHashExceptionMessage"]);
    }

    private async Task DownloadFileAsync(Uri uri, string downloadPath, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken, string? fileName = null) {
        Directory.CreateDirectory(downloadPath);

        if (fileName is null) {
            fileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrEmpty(fileName))
                fileName = localizer["UnknownFileName"];
        }

        var fullPath = Path.Combine(downloadPath, fileName);
        var tempPath = fullPath + ".tmp";

        try {
            using var response =
                await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;

            if (contentLength is null or 0)
                progress.Report(new ProgressReport(-1, localizer["DownloadingFileProgressReportStatusText", fileName]));

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
                                localizer["DownloadingFileProgressReportStatusText", fileName]));
                            lastReport = Stopwatch.GetTimestamp();
                        }
                    }

                    if (contentLength.HasValue && totalRead != contentLength.Value)
                        throw new IOException(localizer["DownloadIncompleteExceptionMessage"]);

                    progress.Report(new ProgressReport(100,
                        localizer["DownloadingFileProgressReportStatusText", fileName]));
                } finally {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            File.Move(tempPath, fullPath, true);
            progress.Report(new ProgressReport(100, localizer["DownloadCompleteProgressReportStatusText"]));
        } catch {
            try {
                File.Delete(tempPath);
            } catch {
                /* ignore */
            }

            throw;
        }
    }
}
