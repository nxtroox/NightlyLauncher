namespace NightlyLauncher.Services;

internal interface IPathService {
    string AppPath { get; }
    string DatabasePath { get; }
    string DownloadPath { get; }
}
