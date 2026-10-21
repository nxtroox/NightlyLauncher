using System;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IAppUpdaterService {
    Task<bool> NeedsUpdateAsync(InstalledApp app, AppManifest manifest,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(InstalledApp app, AppManifest manifest, IProgress<double>? downloadProgress,
        CancellationToken cancellationToken = default);
}
