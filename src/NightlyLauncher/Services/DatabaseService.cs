using System.Text.Json;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Core.Models;
using NightlyLauncher.Serialization;

namespace NightlyLauncher.Services;

internal sealed class DatabaseService(
    IPathService pathService,
    IStringLocalizer<DatabaseService> localizer)
    : IDatabaseService {
    private List<InstalledApp>? _installedApps;

    public async Task<IEnumerable<InstalledApp>> GetInstalledAppsAsync(CancellationToken cancellationToken = default) {
        if (_installedApps is not null)
            return _installedApps;

        try {
            await using var stream = File.Open(pathService.DatabasePath, FileMode.Open, FileAccess.Read);
            _installedApps = await JsonSerializer.DeserializeAsync<List<InstalledApp>>(
                stream, AppJsonContext.Default.ListInstalledApp, cancellationToken) ?? [];
        } catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException) {
            _installedApps = [];
        }

        return _installedApps;
    }

    public async Task RemoveAppAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        var found = await FindAppAsync(app, cancellationToken);

        if (found is not null) {
            _installedApps!.Remove(found);
            await WriteDatabaseAsync(cancellationToken);
        } else
            throw new KeyNotFoundException(localizer["AppNotFoundExceptionMessage", app.Manifest.Guid]);
    }

    public async Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        try {
            await RemoveAppAsync(app, cancellationToken);
        } catch (KeyNotFoundException) {
            /* ignore */
        }

        _installedApps!.Add(app);
        await WriteDatabaseAsync(cancellationToken);
    }

    public InstallOption FindInstallOption(InstalledApp app) {
        var option = app.Manifest.InstallOptions.FirstOrDefault(o => o.Guid == app.InstallOptionGuid);

        if (option is not null) return option;

        var optionName = string.IsNullOrWhiteSpace(app.InstallOptionGuid.ToString())
            ? localizer["UnknownInstallOptionPlaceholder"]
            : app.InstallOptionGuid.ToString();

        throw new InvalidOperationException(localizer["SelectedInstallOptionNotFoundExceptionMessage", optionName]);
    }

    private async Task WriteDatabaseAsync(CancellationToken cancellationToken) {
        var directory = Path.GetDirectoryName(pathService.DatabasePath);

        if (directory is not null)
            Directory.CreateDirectory(directory);

        await using var stream = File.Open(pathService.DatabasePath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, _installedApps, AppJsonContext.Default.ListInstalledApp,
            cancellationToken);
    }

    private async Task<InstalledApp?> FindAppAsync(InstalledApp app, CancellationToken cancellationToken) {
        await GetInstalledAppsAsync(cancellationToken);
        return _installedApps!.FirstOrDefault(a => a.Manifest.Guid == app.Manifest.Guid);
    }
}
