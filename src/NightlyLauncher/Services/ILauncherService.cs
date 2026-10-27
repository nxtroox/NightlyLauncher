using System;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface ILauncherService {
    Task LaunchAppAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default);
}
