using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal sealed class ConfigService(JsonSerializerOptions jsonSerializerOptions, IStringLocalizer<ConfigService> localizer) : IConfigService {
    private Config? _cachedConfig;

    public Config Config =>
        _cachedConfig ?? throw new InvalidOperationException(localizer["ConfigNotLoaded"]);

    public async Task<Config> GetConfigAsync(CancellationToken cancellationToken) =>
        _cachedConfig ??= await LoadAsync(GetConfigPath(), cancellationToken);

    private async Task<Config> LoadAsync(string path, CancellationToken cancellationToken) {
        if (!File.Exists(path)) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var config = new Config();
            await using var stream = File.OpenWrite(path);

            await JsonSerializer.SerializeAsync(stream, config, jsonSerializerOptions, cancellationToken);

            return config;
        } else {
            await using var stream = File.OpenRead(path);

            return await JsonSerializer.DeserializeAsync<Config>(
                stream, jsonSerializerOptions, cancellationToken) ?? new Config();
        }
    }

    private static string GetConfigPath() {
        var appDataConfig = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "NightlyLauncher", "config.json");

        var exeConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "config.json");

        return File.Exists(exeConfig) ? exeConfig : appDataConfig;
    }
}
