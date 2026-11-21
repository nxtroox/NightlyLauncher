using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IConfigService {
    Config Config { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Config updatedConfig, CancellationToken cancellationToken = default);
}
