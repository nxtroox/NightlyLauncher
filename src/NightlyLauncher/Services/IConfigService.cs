using System.Threading;
using System.Threading.Tasks;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface IConfigService {
    Task<Config> GetConfigAsync(CancellationToken cancellationToken = default);
    Config Config { get; }
}
