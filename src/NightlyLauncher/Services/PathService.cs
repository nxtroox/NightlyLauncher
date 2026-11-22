using NightlyLauncher.Core.Models;

namespace NightlyLauncher.Services;

internal sealed class PathService : IPathService {
    public PathService(IConfigService configService) {
        AppPath = ParseRootedPath(configService.Config.AppPath, true);
        CachePath = ParseRootedPath(configService.Config.CachePath, true);
        DatabasePath = Path.Combine(LocalPath, "apps.json");
        DownloadPath = Path.Combine(CachePath, "Downloads");
        IconPath = Path.Combine(CachePath, "Icons");
        RepositoryPath = Path.Combine(CachePath, "Repositories");
    }

    private static string RoamingPath => IsPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher");

    private static string LocalPath => IsPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "NightlyLauncher");

    public string AppPath { get; }
    public string CachePath { get; }
    public string DatabasePath { get; }
    public string DownloadPath { get; }
    public string IconPath { get; }
    public string RepositoryPath { get; }

    public string GetAppPath(InstalledApp app) =>
        Path.Combine(AppPath, app.Manifest.Guid + "_" + app.InstallOptionGuid);

    public string GetRepositoryPath(Guid repositoryGuid) =>
        Path.Combine(RepositoryPath, repositoryGuid + ".json");

    private static bool IsPortable() =>
        File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));

    private static string ParseRootedPath(string path, bool local = false) =>
        Path.IsPathRooted(path) ? path : Path.Combine(local ? LocalPath : RoamingPath, path);
}
