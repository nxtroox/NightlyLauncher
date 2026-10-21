using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NightlyLauncher.Models;
using NightlyLauncher.Services;
using NightlyLauncher.Views.Windows;

namespace NightlyLauncher.ViewModels;

internal partial class MainViewModel(
    IDatabaseService databaseService,
    IConfigService configService,
    IDialogService dialogService) : ViewModelBase {
    public ObservableCollection<InstalledApp> Apps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(LaunchAppCommand))]
    public partial InstalledApp? SelectedApp { get; set; }

    [RelayCommand]
    private async Task AddApp(CancellationToken cancellationToken) {
        var manifest = new AppManifest(Guid.NewGuid(), "Placeholder", new List<InstallOption>());
        var app = new InstalledApp(manifest, UpdatedOn: DateTime.UtcNow);

        await databaseService.UpdateAppAsync(app, cancellationToken);
        await LoadApps(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private void EditApp() => throw new NotImplementedException();

    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private void LaunchApp() => throw new NotImplementedException();

    [RelayCommand]
    private async Task OpenSettings() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            await dialogService.ShowDialogAsync<SettingsWindow, SettingsViewModel, bool>(desktop.MainWindow!);
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await configService.GetConfigAsync(cancellationToken);
        await LoadApps(cancellationToken);
    }

    private bool CanInteractWithApp() => SelectedApp != null;

    private async Task LoadApps(CancellationToken cancellationToken) {
        Apps.Clear();
        foreach (var app in await databaseService.GetAppsAsync(cancellationToken))
            Apps.Add(app);
    }
}
