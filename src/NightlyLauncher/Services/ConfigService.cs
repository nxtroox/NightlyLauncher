using System.Text.Json;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Serialization;

namespace NightlyLauncher.Services;

internal sealed class ConfigService(
    IStringLocalizer<ConfigService> localizer) : IConfigService {
    private readonly string _configPath = GetConfigPath();
    private readonly SemaphoreSlim _gate = new(1, 1);

    private Config? _config;

    public Config Config =>
        _config ?? throw new InvalidOperationException(localizer["ConfigNotInitializedExceptionMessage"]);

    public async Task UpdateAsync(Config updatedConfig, CancellationToken cancellationToken = default) {
        await _gate.WaitAsync(cancellationToken);

        try {
            _config = updatedConfig;
            await SaveAsync(cancellationToken);
        } finally {
            _gate.Release();
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await _gate.WaitAsync(cancellationToken);

        try {
            _config ??= await LoadAsync(_configPath, cancellationToken);
        } finally {
            _gate.Release();
        }
    }

    private async Task<Config> LoadAsync(string path, CancellationToken cancellationToken) {
        if (!File.Exists(path))
            return new Config();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            true);

        return await JsonSerializer.DeserializeAsync<Config>(stream, AppJsonContext.Default.Config,
            cancellationToken) ?? new Config();
    }

    private async Task SaveAsync(CancellationToken cancellationToken) {
        if (_config is null)
            throw new InvalidOperationException(localizer["ConfigNotInitializedExceptionMessage"]);

        await WriteAsync(_config!, _configPath, cancellationToken);
    }

    private async Task WriteAsync(Config config, string path, CancellationToken cancellationToken = default) {
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = path + "." + Guid.NewGuid() + ".tmp";

        try {
            await using (var stream = new FileStream(
                             tempPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             true)) {
                await JsonSerializer.SerializeAsync(stream, config, AppJsonContext.Default.Config, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            File.Move(tempPath, path, true);
        } finally {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static string GetConfigPath() {
        var portableConfig = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "config.json");

        var appDataConfig = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "NightlyLauncher",
            "config.json");

        return File.Exists(portableConfig) ? portableConfig : appDataConfig;
    }
}
