using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class LauncherService(
    IPathService pathService,
    IStringLocalizer<LauncherService> localizer,
    IDatabaseService databaseService,
    IAppUpdaterService updaterService)
    : ILauncherService {
    public async Task StartAppAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        var option = databaseService.FindInstallOption(app);

        await updaterService.ApplyPortableAsync(app, cancellationToken);

        progress.Report(new ProgressReport(-1,
            localizer["LaunchingAppProgressReportStatusText", app.Manifest.DisplayName]));

        var executablePath = Path.Combine(pathService.GetInstallPath(app), option.LaunchCommand.ExecutablePath);

        var startInfo = new ProcessStartInfo { FileName = executablePath, UseShellExecute = true };
        option.LaunchCommand.Arguments?.ForEach(a => startInfo.ArgumentList.Add(a));

        Process.Start(startInfo);
    }

    public Task<bool> OpenFolderAsync(string path) {
        var topLevel = GetTopLevel();

        return topLevel is null
            ? throw new InvalidOperationException()
            : topLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
    }

    public Task<bool> OpenUriAsync(Uri uri) {
        var topLevel = GetTopLevel();

        return topLevel is null
            ? throw new InvalidOperationException()
            : topLevel.Launcher.LaunchUriAsync(uri);
    }

    private static TopLevel? GetTopLevel() =>
        Application.Current?.ApplicationLifetime switch {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };
}
