using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class LauncherService(
    IPathService pathService,
    IStringLocalizer<LauncherService> localizer,
    IDatabaseService databaseService,
    IAppUpdaterService updaterService)
    : ILauncherService {
    public async Task LaunchAppAsync(InstalledApp app, IProgress<ProgressReport> progress,
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
}
