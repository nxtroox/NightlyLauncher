using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class MainViewModel(
    IDatabaseService databaseService,
    IConfigService configService,
    IDialogService dialogService,
    ILauncherService launcherService,
    IStringLocalizer<MainViewModel> localizer,
    IAppUpdaterService appUpdaterService) : ViewModelBase {
    public ObservableCollection<InstalledApp> Apps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(LaunchAppCommand))]
    public partial InstalledApp? SelectedApp { get; set; }

    [RelayCommand]
    private async Task AddApp(CancellationToken cancellationToken) {
        var manifest = new AppManifest(Guid.NewGuid(), "Placeholder", []);
        var app = new InstalledApp(manifest, "1.0.0", UpdatedOn: DateTime.UtcNow);

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
    private void LaunchApp() => launcherService.LaunchApp(SelectedApp!);

    [RelayCommand]
    private async Task OpenSettings() {
        var vm = dialogService.CreateViewModel<SettingsViewModel>();
        await dialogService.ShowDialogAsync(this, vm);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task RemoveApp(CancellationToken cancellationToken = default) {
        var removeAppMessageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = localizer["RemoveAppConfirmationTitle", SelectedApp!.Manifest.ReadableName],
            Content = localizer["RemoveAppConfirmationText", SelectedApp.Manifest.ReadableName]
        };

        var removeAppResult = await dialogService.ShowMessageBoxAsync(this, removeAppMessageBoxSettings);

        if (removeAppResult is true) {
            var removeAppDataMessageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.YesNo,
                DefaultValue = false,
                Icon = MessageBoxImage.Warning,
                Title = localizer["RemoveAppDataConfirmationTitle", SelectedApp.Manifest.ReadableName],
                Content = localizer["RemoveAppDataConfirmationText", SelectedApp.Manifest.ReadableName]
            };

            var removeAppDataResult = await dialogService.ShowMessageBoxAsync(this, removeAppDataMessageBoxSettings);

            if (removeAppDataResult is true) {
                appUpdaterService.DeleteAppData(SelectedApp);
            }

            await databaseService.RemoveAppAsync(SelectedApp, cancellationToken);
            await LoadApps(cancellationToken);
        }
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
