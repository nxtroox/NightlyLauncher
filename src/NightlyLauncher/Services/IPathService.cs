using NightlyLauncher.Core.Models;

namespace NightlyLauncher.Services;

internal interface IPathService {
    string AppPath { get; }
    string DatabasePath { get; }
    string DownloadPath { get; }
    string IconPath { get; }
    string RepositoryPath { get; }
    string GetAppPath(InstalledApp app);
}
