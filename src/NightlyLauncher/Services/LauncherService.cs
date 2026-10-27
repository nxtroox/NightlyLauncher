using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class LauncherService(
    IPathService pathService,
    IStringLocalizer<LauncherService> localizer,
    IAppUpdaterService updaterService)
    : ILauncherService {
    public void LaunchApp(InstalledApp app, IProgress<ProgressReport> progress) {
        var option = updaterService.FindInstallOption(app);

        progress.Report(new ProgressReport(-1, localizer["LaunchingApp", app.Manifest.ReadableName]));

        var executablePath = Path.Combine(pathService.GetInstallPath(app), option.LaunchCommand.ExecutablePath);

        var pi = new ProcessStartInfo { FileName = executablePath, UseShellExecute = true };
        option.LaunchCommand.Arguments?.ForEach(a => pi.ArgumentList.Add(a));

        Process.Start(pi);
    }
}
