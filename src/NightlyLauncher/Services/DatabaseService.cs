using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class DatabaseService(IPathService pathService, JsonSerializerOptions jsonSerializerOptions) : IDatabaseService {
    private List<InstalledApp>? _installedApps;

    private async Task<List<InstalledApp>> LoadAsync(CancellationToken cancellationToken) {
        List<InstalledApp> apps;

        try {
            await using var stream = File.Open(pathService.DatabasePath, FileMode.Open, FileAccess.Read);
            apps = await JsonSerializer.DeserializeAsync<List<InstalledApp>>(
                stream, jsonSerializerOptions, cancellationToken) ?? [];
        } catch {
            apps = [];
        }

        return apps;
    }

    public Task<string?> GetLocalVersionAsync(AppManifest manifest, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public async Task UpdateAppAsync(InstalledApp app, CancellationToken cancellationToken = default) {
        _installedApps ??= await LoadAsync(cancellationToken);

        var found = _installedApps.FirstOrDefault(a => a.Manifest == app.Manifest);

        if (found is not null)
            _installedApps.Remove(found);

        _installedApps.Add(app);

        await using var stream = File.Open(pathService.DatabasePath, FileMode.OpenOrCreate, FileAccess.Write);

        await JsonSerializer.SerializeAsync(stream, _installedApps, jsonSerializerOptions, cancellationToken);
    }

    public async Task<IEnumerable<InstalledApp>> GetAppsAsync(CancellationToken cancellationToken = default) =>
        _installedApps ??= await LoadAsync(cancellationToken);
}
