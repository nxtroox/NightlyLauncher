using System;
using System.Collections.ObjectModel;
using System.IO;
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
    private async Task AddApp(CancellationToken cancellationToken) {
        var installGuid = Guid.NewGuid();

        var manifest = new AppManifest(Guid.NewGuid(), "Cemu", [
                new InstallOption(installGuid,
                    new Uri(
                        "https://nightly.link/cemu-project/Cemu/workflows/build_check/main/cemu-bin-windows-x64.zip"),
                    new Uri("https://api.github.com/repos/cemu-project/Cemu/commits/main"),
                    new LaunchCommand("Cemu.exe"),
                    "(win-x64, zip)",
                    new PortablePath("portable", PortablePathType.Folder))
            ],
            true, true);
        var app = new InstalledApp(manifest, "0", installGuid);

        await databaseService.UpdateAppAsync(app, cancellationToken);
        await LoadApps(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task EditApp(CancellationToken cancellationToken = default) {
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
    private async Task LaunchApp(CancellationToken cancellationToken = default) {
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
            var shouldUpdate = SelectedApp!.UpdatedOn is null || needsUpdate;

            if (shouldUpdate && SelectedApp.UpdatedOn is not null && SelectedApp.Manifest.UpdatePrompt) {
                var updatePrompt = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    Content = localizer["UpdatePromptText", SelectedApp.Manifest.ReadableName],
                    Icon = MessageBoxImage.Information,
                    DefaultValue = false,
                    Title = localizer["UpdatePromptTitle", SelectedApp.Manifest.ReadableName]
                };

                shouldUpdate = await dialogService.ShowMessageBoxAsync(this, updatePrompt) is true;
            }

            if (shouldUpdate)
                await updaterService.UpdateAsync(SelectedApp!, progress, cancellationToken);

            await launcherService.LaunchAppAsync(SelectedApp!, progress, cancellationToken);
        } catch (Exception ex) {
            var messageBox = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = localizer["ErrorMessageBoxTitle", SelectedApp!.Manifest.ReadableName]
            };

            await dialogService.ShowMessageBoxAsync(this, messageBox);
        } finally {
            vm.Close(true);
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
            if (Directory.Exists(pathService.GetInstallPath(SelectedApp))) {
                var removeAppDataMessageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    DefaultValue = false,
                    Icon = MessageBoxImage.Warning,
                    Title = localizer["RemoveAppDataConfirmationTitle", SelectedApp.Manifest.ReadableName],
                    Content = localizer["RemoveAppDataConfirmationText", SelectedApp.Manifest.ReadableName]
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
    private async Task UpdateRepositories(CancellationToken cancellationToken = default) =>
        await repositoryService.UpdateRepositoriesAsync(cancellationToken);

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
