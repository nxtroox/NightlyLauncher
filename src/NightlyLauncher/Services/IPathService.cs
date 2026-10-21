using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IPathService {
    string DatabasePath { get; }
    string DownloadPath { get; }
    string GetInstallPath(InstalledApp app);
}
