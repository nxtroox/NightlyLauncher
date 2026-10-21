using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IConfigService {
    Config Config { get; }
    Task<Config> GetConfigAsync(CancellationToken cancellationToken = default);
    Task UpdateConfigAsync(Config config, CancellationToken cancellationToken = default);
}
