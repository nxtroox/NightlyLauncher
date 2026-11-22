using System.Text.Json;
using NightlyLauncher.Models;
using NightlyLauncher.Serialization;

namespace NightlyLauncher.Services;

internal sealed class ConfigService : IConfigService {
    private readonly string _configPath = GetConfigPath();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ConfigService() => Config = LoadConfig(_configPath);

    public Config Config { get; private set; }

    public async Task UpdateAsync(Config updatedConfig, CancellationToken cancellationToken = default) {
        await _gate.WaitAsync(cancellationToken);

        try {
            Config = updatedConfig;
            await WriteAsync(cancellationToken);
        } finally {
            _gate.Release();
        }
    }

    private static Config LoadConfig(string path) {
        if (!File.Exists(path))
            return new Config();

        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096);

        return JsonSerializer.Deserialize<Config>(stream, AppJsonContext.Default.Config) ?? new Config();
    }

    private async Task WriteAsync(CancellationToken cancellationToken = default) {
        var directory = Path.GetDirectoryName(_configPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = _configPath + "." + Guid.NewGuid() + ".tmp";

        try {
            await using (var stream = new FileStream(
                             tempPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             true)) {
                await JsonSerializer.SerializeAsync(stream, Config, AppJsonContext.Default.Config, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            File.Move(tempPath, _configPath, true);
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
