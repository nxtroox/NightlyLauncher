using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    IAppUpdaterService updaterService,
    IPathService pathService,
    IRepositoryService repositoryService) : ViewModelBase {
    public ObservableCollection<InstalledApp> Apps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(LaunchAppCommand))]
    public partial InstalledApp? SelectedApp { get; set; }

    [RelayCommand]
    private async Task AddApps(CancellationToken cancellationToken) {
        var manifests = await repositoryService.GetAvailableAppsAsync(cancellationToken);
        var apps = manifests
            .Where(manifest => Apps.All(a => a.Manifest.Guid != manifest.Guid))
            .ToList();

        if (apps.Count is 0) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = localizer["NoAppsAvailableMessageBoxContent"],
                DefaultValue = true,
                Icon = MessageBoxImage.Information,
                Title = localizer["NoAppsAvailableMessageBoxTitle"]
            };

            await dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
            return;
        }

        var vm = dialogService.CreateViewModel<AppBrowserViewModel>();

        foreach (var app in apps)
            vm.Apps.Add(app);

        var result = await dialogService.ShowDialogAsync(this, vm);

        if (result is true) {
            var app = new InstalledApp(vm.SelectedApp!,
                vm.SelectedInstallOptionGuid ??
                throw new InvalidOperationException(localizer["NoInstallOptionSelectedExceptionMessage"]));

            await databaseService.UpdateAppAsync(app, cancellationToken);
            await LoadApps(cancellationToken);
        }
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task EditApp(CancellationToken cancellationToken) {
        var vm = dialogService.CreateViewModel<EditViewModel>();
        vm.SelectedApp = SelectedApp!;

        var result = await dialogService.ShowDialogAsync(this, vm);
        if (result is not true) return;

        await databaseService.UpdateAppAsync(vm.SelectedApp, cancellationToken);
        await LoadApps(cancellationToken);
    }

    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task LaunchApp(CancellationToken cancellationToken) {
        var vm = dialogService.CreateViewModel<UpdaterViewModel>();

        var progress = new Progress<ProgressReport>(report => {
            vm.StatusText = report.StatusText;

            vm.ProgressIsIndeterminate = report.Percentage < 0;
            if (report.Percentage >= 0)
                vm.ProgressValue = report.Percentage;
        });

        var dialogTask = dialogService.ShowDialogAsync(this, vm);

        try {
            var needsUpdate = await updaterService.NeedsUpdateAsync(SelectedApp!, progress, cancellationToken);
            var shouldUpdate = SelectedApp!.LastUpdatedOn is null || needsUpdate;

            if (shouldUpdate && SelectedApp.LastUpdatedOn is not null && SelectedApp.Manifest.UpdatePrompt) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    Content = localizer["UpdatePromptMessageBoxContent", SelectedApp.Manifest.DisplayName],
                    Icon = MessageBoxImage.Information,
                    DefaultValue = false,
                    Title = localizer["UpdatePromptMessageBoxTitle", SelectedApp.Manifest.DisplayName]
                };

                shouldUpdate = await dialogService.ShowMessageBoxAsync(this, messageBoxSettings) is true;
            }

            if (shouldUpdate)
                await updaterService.UpdateAsync(SelectedApp!, progress, cancellationToken);

            await launcherService.LaunchAppAsync(SelectedApp!, progress, cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = localizer["AppUpdateErrorMessageBoxTitle", SelectedApp!.Manifest.DisplayName]
            };

            await dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
        } finally {
            vm.OkCommand.Execute(null);
            await dialogTask;
        }

        await LoadApps(cancellationToken);
    }

    [RelayCommand]
    private async Task OpenSettings() {
        var vm = dialogService.CreateViewModel<SettingsViewModel>();
        await dialogService.ShowDialogAsync(this, vm);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task RemoveApp(CancellationToken cancellationToken) {
        var removeAppMessageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = localizer["RemoveAppMessageBoxTitle", SelectedApp!.Manifest.DisplayName],
            Content = localizer["RemoveAppMessageBoxContent", SelectedApp.Manifest.DisplayName]
        };

        var removeAppResult = await dialogService.ShowMessageBoxAsync(this, removeAppMessageBoxSettings);

        if (removeAppResult is true) {
            if (Directory.Exists(pathService.GetInstallPath(SelectedApp))) {
                var removeAppDataMessageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    DefaultValue = false,
                    Icon = MessageBoxImage.Warning,
                    Title = localizer["RemoveAppMessageBoxTitle", SelectedApp.Manifest.DisplayName],
                    Content = localizer["RemoveAppDataMessageBoxContent", SelectedApp.Manifest.DisplayName]
                };

                var removeAppDataResult =
                    await dialogService.ShowMessageBoxAsync(this, removeAppDataMessageBoxSettings);

                if (removeAppDataResult is true) {
                    updaterService.DeleteAppData(SelectedApp);
                }
            }

            await databaseService.RemoveAppAsync(SelectedApp, cancellationToken);
            await LoadApps(cancellationToken);
        }
    }

    [RelayCommand]
    private async Task UpdateRepositories(CancellationToken cancellationToken) {
        // TODO: dont stop updating when only 1 repository throws exceptions
        try {
            await repositoryService.UpdateRepositoriesAsync(cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = localizer["RepositoryUpdateErrorMessageBoxTitle"]
            };

            await dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
        }
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await configService.GetConfigAsync(cancellationToken);
        await LoadApps(cancellationToken);
    }

    private bool CanInteractWithApp() => SelectedApp != null;

    private async Task LoadApps(CancellationToken cancellationToken) {
        Apps.Clear();
        foreach (var app in await databaseService.GetInstalledAppsAsync(cancellationToken))
            Apps.Add(app);
    }
}
