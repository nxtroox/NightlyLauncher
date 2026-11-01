using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;
using OperatingSystem = NightlyLauncher.Models.OperatingSystem;

namespace NightlyLauncher.Services;

internal interface IRepositoryService {
    Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default);
    bool CheckAppCompatibility(OperatingSystem requestedOperatingSystem, Architecture requestedArchitecture);
    Task<List<AppManifest>> GetAvailableAppsAsync(CancellationToken cancellationToken = default);
    Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default);
    Task UpdateRepositoriesAsync(CancellationToken cancellationToken = default);
}
