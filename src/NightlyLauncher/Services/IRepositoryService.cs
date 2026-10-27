using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IRepositoryService {
    Task AddRepositoryAsync(Uri uri, CancellationToken cancellationToken = default);
    Task<List<AppManifest>?> GetAvailableAppsAsync(CancellationToken cancellationToken = default);
    Task RemoveRepositoryAsync(Repository repository, CancellationToken cancellationToken = default);
    Task UpdateRepositoriesAsync(CancellationToken cancellationToken = default);
}
