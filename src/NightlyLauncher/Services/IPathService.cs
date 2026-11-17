using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IPathService {
    string AppPath { get; }
    string DatabasePath { get; }
    string DownloadPath { get; }
    string RepositoryCachePath { get; }
    string GetAppPath(InstalledApp app);
}
