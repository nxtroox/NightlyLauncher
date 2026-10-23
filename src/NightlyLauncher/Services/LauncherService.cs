using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class LauncherService(
    IPathService pathService,
    IStringLocalizer<LauncherService> localizer,
    IAppUpdaterService updaterService)
    : ILauncherService {
    public async Task LaunchAppAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default) {
        var option = app.Manifest.InstallOptions.FirstOrDefault(o => o.Guid == app.SelectedInstallOptionGuid);

        if (option is null) {
            var optionName = string.IsNullOrWhiteSpace(app.SelectedInstallOptionGuid.ToString())
                ? localizer["UnknownInstallOption"]
                : app.SelectedInstallOptionGuid.ToString();

            throw new InvalidOperationException(localizer["InstallOptionNotFound", optionName]);
        }

        if (await updaterService.NeedsUpdateAsync(app, progress, cancellationToken))
            await updaterService.UpdateAsync(app, progress, cancellationToken);

        progress.Report(new ProgressReport(-1, localizer["LaunchingApp", app.Manifest.ReadableName]));

        var executablePath = Path.Combine(pathService.GetInstallPath(app), option.LaunchCommand.ExecutablePath);

        var pi = new ProcessStartInfo { FileName = executablePath, UseShellExecute = true };
        option.LaunchCommand.Arguments?.ForEach(a => pi.ArgumentList.Add(a));

        Process.Start(pi);
    }
}
