using System;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IAppUpdaterService {
    void DeleteAppData(InstalledApp app);

    Task<bool> NeedsUpdateAsync(InstalledApp app, CancellationToken cancellationToken = default);

    Task UpdateAsync(InstalledApp app, IProgress<double>? downloadProgress,
        CancellationToken cancellationToken = default);
}
