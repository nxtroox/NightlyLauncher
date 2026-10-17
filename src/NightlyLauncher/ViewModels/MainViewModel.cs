using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class MainViewModel(IDatabaseService databaseService, IConfigService configService) : ViewModelBase {
    public ObservableCollection<InstalledApp> Apps { get; } = [];

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await configService.GetConfigAsync(cancellationToken);

        foreach (var app in await databaseService.GetAppsAsync(cancellationToken))
            Apps.Add(app);
    }

    [RelayCommand]
    private async Task AddApp(CancellationToken cancellationToken) {
        var manifest = new AppManifest(new List<InstallOperation>(), "Placeholder");

        var app = new InstalledApp(manifest);

        await databaseService.UpdateAppAsync(app, cancellationToken);
        await LoadApps(cancellationToken);
    }

    private async Task LoadApps(CancellationToken cancellationToken) {
        Apps.Clear();

        foreach (var app in await databaseService.GetAppsAsync(cancellationToken))
            Apps.Add(app);
    }
}
