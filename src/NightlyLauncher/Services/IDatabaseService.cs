using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IDatabaseService {
    Task<IEnumerable<InstalledApp>> GetAppsAsync(CancellationToken cancellationToken = default);
    Task RemoveAppAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default);
}
