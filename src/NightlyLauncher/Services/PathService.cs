using System;
using System.IO;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class PathService(IConfigService configService) : IPathService {
    private static string BasePath => IsPortable()
        ? AppDomain.CurrentDomain.BaseDirectory
        : Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher");

    public string DatabasePath => Path.Combine(BasePath, "apps.json");

    public string DownloadPath => ParseRootedPath(configService.Config.DownloadPath);

    public string GetInstallPath(InstalledApp app) =>
        Path.Combine(ParseRootedPath(configService.Config.AppPath),
            app.Manifest.Guid + "_" + app.Manifest.SelectedInstallOptionGuid);

    private static bool IsPortable() {
        var exeConfig = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "config.json");

        return File.Exists(exeConfig);
    }

    private static string ParseRootedPath(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(BasePath, path);
}
