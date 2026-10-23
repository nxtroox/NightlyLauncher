using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class DatabaseService(
    IPathService pathService,
    JsonSerializerOptions jsonSerializerOptions,
    IStringLocalizer<DatabaseService> localizer)
    : IDatabaseService {
    private List<InstalledApp>? _installedApps;

    public async Task<IEnumerable<InstalledApp>> GetAppsAsync(CancellationToken cancellationToken = default) {
        if (_installedApps is not null)
            return _installedApps;

        try {
            await using var stream = File.Open(pathService.DatabasePath, FileMode.Open, FileAccess.Read);
            _installedApps = await JsonSerializer.DeserializeAsync<List<InstalledApp>>(
                stream, jsonSerializerOptions, cancellationToken) ?? [];
        } catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException) {
            _installedApps = [];
        }

        return _installedApps;
    }

    public async Task RemoveAppAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        var found = await FindAppAsync(app, cancellationToken);

        if (found is not null) {
            _installedApps!.Remove(found);
            await UpdateDatabaseAsync(cancellationToken);
        } else
            throw new KeyNotFoundException(localizer["AppNotFound", app.Manifest.Guid]);
    }

    public async Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        try {
            await RemoveAppAsync(app, cancellationToken);
        } catch (KeyNotFoundException) {
            /* ignore */
        }

        _installedApps!.Add(app);
        await UpdateDatabaseAsync(cancellationToken);
    }

    private async Task UpdateDatabaseAsync(CancellationToken cancellationToken) {
        await using var stream = File.Open(pathService.DatabasePath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, _installedApps, jsonSerializerOptions, cancellationToken);
    }

    private async Task<InstalledApp?> FindAppAsync(InstalledApp app, CancellationToken cancellationToken) {
        await GetAppsAsync(cancellationToken);
        return _installedApps!.FirstOrDefault(a => a.Manifest.Guid == app.Manifest.Guid);
    }
}
