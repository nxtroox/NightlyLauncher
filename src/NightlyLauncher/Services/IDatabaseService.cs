using NightlyLauncher.Core.Models;

namespace NightlyLauncher.Services;

internal interface IDatabaseService {
    InstallOption FindInstallOption(InstalledApp app);
    Task<IEnumerable<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default);
    Task RemoveAppAsync(InstalledApp app, CancellationToken cancellationToken = default);
    Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default);
}
