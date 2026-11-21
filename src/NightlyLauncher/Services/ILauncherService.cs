using NightlyLauncher.Core.Models;

namespace NightlyLauncher.Services;

internal interface ILauncherService {
    Task StartAppAsync(InstalledApp app, IProgress<ProgressReport> progress,
        CancellationToken cancellationToken = default);

    Task<bool> OpenFolderAsync(string path);

    Task<bool> OpenUriAsync(Uri uri);
}
