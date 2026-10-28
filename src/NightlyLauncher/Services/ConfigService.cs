using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class ConfigService(
    JsonSerializerOptions jsonSerializerOptions,
    IStringLocalizer<ConfigService> localizer) : IConfigService {
    private Config? _cachedConfig;

    public Config Config => _cachedConfig ?? throw new InvalidOperationException(localizer["ConfigNotLoaded"]);

    public async Task<Config> GetConfigAsync(CancellationToken cancellationToken) =>
        _cachedConfig ??= await LoadAsync(GetConfigPath(), cancellationToken);

    public async Task UpdateConfigAsync(Config config, CancellationToken cancellationToken = default) {
        _cachedConfig = config;
        await WriteConfigAsync(config, GetConfigPath(), cancellationToken);
    }

    private async Task WriteConfigAsync(Config config, string path, CancellationToken cancellationToken) {
        var directory = Path.GetDirectoryName(path);

        if (directory is not null)
            Directory.CreateDirectory(directory);

        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, config, jsonSerializerOptions, cancellationToken);
    }

    private async Task<Config> LoadAsync(string path, CancellationToken cancellationToken) {
        if (!File.Exists(path)) {
            var config = new Config();
            await WriteConfigAsync(config, path, cancellationToken);

            return config;
        }

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<Config>(
            stream, jsonSerializerOptions, cancellationToken) ?? new Config();
    }

    // this is here so that DataBaseService and ConfigService don't depend on each other
    private static string GetConfigPath() {
        var appDataConfig = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher", "config.json");

        var exeConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "config.json");

        return File.Exists(exeConfig) ? exeConfig : appDataConfig;
    }
}
