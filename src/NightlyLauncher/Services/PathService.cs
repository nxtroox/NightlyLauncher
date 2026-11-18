using System;
using System.IO;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class PathService(IConfigService configService) : IPathService {
    private static string RoamingPath => IsPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher");

    private static string LocalPath => IsPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "NightlyLauncher");

    public string AppPath => ParseRootedPath(configService.Config.AppPath, true);

    public string DatabasePath => Path.Combine(LocalPath, "apps.json");

    public string DownloadPath => ParseRootedPath(configService.Config.DownloadPath, true);

    public string RepositoryCachePath => Path.Combine(LocalPath, "RepositoryCache");

    public string GetAppPath(InstalledApp app) =>
        Path.Combine(ParseRootedPath(configService.Config.AppPath, true),
            app.Manifest.Guid + "_" + app.InstallOptionGuid);

    private static bool IsPortable() {
        var exeConfig = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "config.json");

        return File.Exists(exeConfig);
    }

    private static string ParseRootedPath(string path, bool local = false) =>
        Path.IsPathRooted(path) ? path : Path.Combine(local ? LocalPath : RoamingPath, path);
}
