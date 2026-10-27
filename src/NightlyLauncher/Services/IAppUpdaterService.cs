using System;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IAppUpdaterService {
    void DeleteAppData(InstalledApp app);

    Task ApplyPortableAsync(InstalledApp app, CancellationToken cancellationToken = default);

    Task<bool> NeedsUpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default);
}
