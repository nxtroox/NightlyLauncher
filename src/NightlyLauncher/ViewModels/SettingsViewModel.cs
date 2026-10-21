using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class SettingsViewModel(IConfigService configService) : ViewModelBase {
    [ObservableProperty]
    public partial Config? Config { get; set; }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await configService.GetConfigAsync(cancellationToken);
}
