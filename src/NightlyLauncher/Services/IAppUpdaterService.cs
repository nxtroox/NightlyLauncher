using System;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IAppUpdaterService {
    Task<bool> NeedsUpdateAsync(InstallOperation operation, AppManifest manifest, CancellationToken cancellationToken = default);
    Task UpdateAsync(InstallOperation operation, AppManifest manifest, IProgress<double>? downloadProgress, CancellationToken cancellationToken = default);
}
