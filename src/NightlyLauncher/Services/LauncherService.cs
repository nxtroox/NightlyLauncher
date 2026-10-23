using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class LauncherService(IPathService pathService, IStringLocalizer<LauncherService> localizer)
    : ILauncherService {
    // needs to implement check for updates/AppUpdaterService
    public void LaunchApp(InstalledApp app) {
        var option = app.Manifest.InstallOptions.FirstOrDefault(o => o.Guid == app.SelectedInstallOptionGuid);

        if (option is null)
            throw new InvalidOperationException(localizer["InstallOptionNotFound",
                app.SelectedInstallOptionGuid.ToString() ?? localizer["UnknownInstallOption"]]);

        var executablePath = Path.Combine(pathService.GetInstallPath(app), option.LaunchCommand.ExecutablePath);

        var pi = new ProcessStartInfo {
            FileName = executablePath,
            Arguments = string.Join(" ", option.LaunchCommand.Arguments),
            UseShellExecute = true
        };

        Process.Start(pi);
    }
}
