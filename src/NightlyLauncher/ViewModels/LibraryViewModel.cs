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
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Models.UI;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal sealed partial class LibraryViewModel : ViewModelBase {
    private readonly IDatabaseService _databaseService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly ILauncherService _launcherService;
    private readonly IStringLocalizer<LibraryViewModel> _localizer;
    private readonly IPathService _pathService;
    private readonly IRepositoryService _repositoryService;
    private readonly IAppUpdaterService _updaterService;

    public LibraryViewModel(IDatabaseService databaseService,
        DialogOwner dialogOwner,
        IDialogService dialogService,
        ILauncherService launcherService,
        IStringLocalizer<LibraryViewModel> localizer,
        IPathService pathService,
        IRepositoryService repositoryService,
        IAppUpdaterService updaterService) {
        _databaseService = databaseService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _launcherService = launcherService;
        _localizer = localizer;
        _pathService = pathService;
        _repositoryService = repositoryService;
        _updaterService = updaterService;

        Title = _localizer["LibraryViewTitle"];
    }

    public ObservableCollection<InstalledAppWithIcon> Apps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(LaunchAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenAppFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveAppCommand))]
    public partial InstalledAppWithIcon? SelectedApp { get; set; }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task EditApp(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<EditDialogViewModel>();
        vm.SelectedApp = SelectedApp!.App;

        var contentDialogSettings = new ContentDialogSettings {
            CloseButtonText = _localizer["EditAppContentDialogCloseButtonText"],
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = _localizer["EditAppContentDialogPrimaryButtonText"],
            Title = _localizer["EditDialogViewTitle", SelectedApp!.App.Manifest.DisplayName]
        };

        var result = await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings);
        if (result != FAContentDialogResult.Primary) return;

        await _databaseService.UpdateAppAsync(vm.SelectedApp, cancellationToken);
        await LoadAppsAsync(cancellationToken);
    }

    [RelayCommand]
    private static void Exit() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task LaunchApp(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<UpdaterViewModel>();

        var progress = new Progress<ProgressReport>(report => {
            vm.StatusText = report.StatusText;

            vm.ProgressIsIndeterminate = report.Percentage < 0;
            if (report.Percentage >= 0)
                vm.ProgressValue = report.Percentage;
        });

        var dialogTask = _dialogService.ShowDialogAsync(_dialogOwner.Window!, vm);

        try {
            var needsUpdate = await _updaterService.NeedsUpdateAsync(SelectedApp!.App, progress, cancellationToken);
            var shouldUpdate = SelectedApp!.App.LastUpdatedOn is null || needsUpdate;

            if (shouldUpdate && SelectedApp!.App.LastUpdatedOn is not null && SelectedApp!.App.Options.UpdatePrompt) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    Content = _localizer["UpdatePromptMessageBoxContent", SelectedApp!.App.Manifest.DisplayName],
                    Icon = MessageBoxImage.Information,
                    DefaultValue = false,
                    Title = _localizer["UpdatePromptMessageBoxTitle", SelectedApp!.App.Manifest.DisplayName]
                };

                shouldUpdate =
                    await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings) is true;
            }

            if (shouldUpdate)
                await _updaterService.UpdateAsync(SelectedApp!.App, progress, cancellationToken);

            await _launcherService.StartAppAsync(SelectedApp!.App, progress, cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = _localizer["AppUpdateErrorMessageBoxTitle", SelectedApp!.App.Manifest.DisplayName]
            };

            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings);
        } finally {
            vm.Close();
            await dialogTask;
        }

        await LoadAppsAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private Task OpenAppFolder() => OpenFolderAsync(_pathService.GetAppPath(SelectedApp?.App!));

    [RelayCommand]
    private Task OpenAppsFolder() => OpenFolderAsync(_pathService.AppPath);

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task RemoveApp(CancellationToken cancellationToken) {
        var removeAppMessageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = _localizer["RemoveAppMessageBoxTitle", SelectedApp!.App.Manifest.DisplayName],
            Content = _localizer["RemoveAppMessageBoxContent", SelectedApp!.App.Manifest.DisplayName]
        };

        var removeAppResult =
            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, removeAppMessageBoxSettings);

        if (removeAppResult is true) {
            if (Directory.Exists(_pathService.GetAppPath(SelectedApp!.App)) || Directory
                    .EnumerateFiles(_pathService.IconPath, SelectedApp!.App.Manifest.Guid + "*").ToList()
                    .Any(File.Exists)) {
                var removeAppDataMessageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    DefaultValue = false,
                    Icon = MessageBoxImage.Warning,
                    Title = _localizer["RemoveAppMessageBoxTitle", SelectedApp!.App.Manifest.DisplayName],
                    Content = _localizer["RemoveAppDataMessageBoxContent", SelectedApp!.App.Manifest.DisplayName]
                };

                var removeAppDataResult =
                    await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!,
                        removeAppDataMessageBoxSettings);

                if (removeAppDataResult is true)
                    _updaterService.DeleteAppData(SelectedApp!.App);
            }

            await _databaseService.RemoveAppAsync(SelectedApp!.App, cancellationToken);
            await LoadAppsAsync(cancellationToken);
        }
    }

    [RelayCommand]
    private Task UpdateRepositories(CancellationToken cancellationToken) =>
        _repositoryService.UpdateRepositoriesAsync(async exception => {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = exception.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = _localizer["RepositoryUpdateErrorMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings);
        }, cancellationToken);

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        await LoadAppsAsync(cancellationToken);

    private bool CanInteractWithApp() => SelectedApp is not null;

    private async Task LoadAppsAsync(CancellationToken cancellationToken) {
        var selectedGuid = SelectedApp?.App.Manifest.Guid;
        var installedApps = await _databaseService.GetInstalledAppsAsync(cancellationToken);

        Apps.Clear();

        var appsWithIcons = installedApps
            .Select(app => new InstalledAppWithIcon(
                app,
                app.Manifest.IconUri is not null
                    ? Directory.EnumerateFiles(_pathService.IconPath, app.Manifest.Guid + "*")
                        .FirstOrDefault()
                    : null
            ));

        foreach (var app in appsWithIcons)
            Apps.Add(app);

        SelectedApp = selectedGuid is not null
            ? Apps.FirstOrDefault(app => app.App.Manifest.Guid == selectedGuid)
            : null;
    }

    private async Task OpenFolderAsync(string path) {
        try {
            await _launcherService.OpenFolderAsync(path);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                DefaultValue = true,
                Icon = MessageBoxImage.Error,
                Title = _localizer["OpenFolderErrorMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings);
        }
    }
}
