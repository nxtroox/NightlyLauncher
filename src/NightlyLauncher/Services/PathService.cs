using System;
using System.IO;

namespace NightlyLauncher.Services;

internal sealed class PathService(IConfigService configService) : IPathService {
    private static bool isPortable() {
        var exeConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "config.json");

        return File.Exists(exeConfig);
    }

    private static string ParseRootedPath(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(BasePath, path);

    private static string BasePath => isPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher");

    public string AppPath => ParseRootedPath(configService.Config.AppPath);

    public string DatabasePath => Path.Combine(BasePath, "apps.json");

    public string DownloadPath => ParseRootedPath(configService.Config.DownloadPath);
}
