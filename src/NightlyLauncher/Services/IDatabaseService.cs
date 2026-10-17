using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IDatabaseService {
    Task<string?> GetLocalVersionAsync(AppManifest manifest, CancellationToken cancellationToken = default);
    Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task<IEnumerable<InstalledApp>> GetAppsAsync(CancellationToken cancellationToken = default);
}
