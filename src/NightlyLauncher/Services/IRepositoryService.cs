using System.Runtime.InteropServices;
using NightlyLauncher.Core.Models;

namespace NightlyLauncher.Services;

internal interface IRepositoryService {
    Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default);
    bool CheckAppCompatibility(OsType requestedOsType, Architecture requestedArchitecture);
    Task<List<AppManifest>> GetAvailableAppsAsync(CancellationToken cancellationToken = default);
    Task<List<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default);
    Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default);
    Task UpdateRepositoriesAsync(Func<Exception, Task> errorCallback, CancellationToken cancellationToken = default);
}
