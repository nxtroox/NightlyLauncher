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

internal partial class MainViewModel : ViewModelBase {
    private readonly IConfigService _configService;
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService _dialogService;
    private readonly IIoDialogService _ioDialogService;
    private readonly ILauncherService _launcherService;
    private readonly IStringLocalizer<MainViewModel> _localizer;
    private readonly IPathService _pathService;
    private readonly IRepositoryService _repositoryService;
    private readonly IAppUpdaterService _updaterService;

    public MainViewModel(IConfigService configService,
        IDatabaseService databaseService,
        IDialogService dialogService,
        ILauncherService launcherService,
        IIoDialogService ioDialogService,
        IStringLocalizer<MainViewModel> localizer,
        IPathService pathService,
        IRepositoryService repositoryService,
        IAppUpdaterService updaterService) {
        _configService = configService;
        _databaseService = databaseService;
        _dialogService = dialogService;
        _launcherService = launcherService;
        _ioDialogService = ioDialogService;
        _localizer = localizer;
        _pathService = pathService;
        _repositoryService = repositoryService;
        _updaterService = updaterService;

        Title = _localizer["MainWindowTitle"];
    }

    public ObservableCollection<InstalledApp> Apps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditAppCommand))]
    [NotifyCanExecuteChangedFor(nameof(LaunchAppCommand))]
    public partial InstalledApp? SelectedApp { get; set; }

    [RelayCommand]
    private async Task AddApps(CancellationToken cancellationToken) {
        var manifests = await _repositoryService.GetAvailableAppsAsync(cancellationToken);
        var apps = manifests
            .Where(manifest => Apps.All(a => a.Manifest.Guid != manifest.Guid))
            .ToList();

        if (apps.Count is 0) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = _localizer["NoAppsAvailableMessageBoxContent"],
                DefaultValue = true,
                Icon = MessageBoxImage.Information,
                Title = _localizer["NoAppsAvailableMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
            return;
        }

        var vm = _dialogService.CreateViewModel<AppBrowserViewModel>();

        foreach (var app in apps)
            vm.Apps.Add(app);

        var result = await _dialogService.ShowDialogAsync(this, vm);

        if (result is true) {
            var app = new InstalledApp(vm.SelectedApp!,
                vm.SelectedInstallOptionGuid ??
                throw new InvalidOperationException(_localizer["NoInstallOptionSelectedExceptionMessage"]));

            await _databaseService.UpdateAppAsync(app, cancellationToken);
            await LoadApps(cancellationToken);
        }
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task EditApp(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<EditViewModel>();
        vm.SelectedApp = SelectedApp!;

        var result = await _dialogService.ShowDialogAsync(this, vm);
        if (result is not true) return;

        await _databaseService.UpdateAppAsync(vm.SelectedApp, cancellationToken);
        await LoadApps(cancellationToken);
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

        var dialogTask = _dialogService.ShowDialogAsync(this, vm);

        try {
            var needsUpdate = await _updaterService.NeedsUpdateAsync(SelectedApp!, progress, cancellationToken);
            var shouldUpdate = SelectedApp!.LastUpdatedOn is null || needsUpdate;

            if (shouldUpdate && SelectedApp.LastUpdatedOn is not null && SelectedApp.Manifest.UpdatePrompt) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    Content = _localizer["UpdatePromptMessageBoxContent", SelectedApp.Manifest.DisplayName],
                    Icon = MessageBoxImage.Information,
                    DefaultValue = false,
                    Title = _localizer["UpdatePromptMessageBoxTitle", SelectedApp.Manifest.DisplayName]
                };

                shouldUpdate = await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings) is true;
            }

            if (shouldUpdate)
                await _updaterService.UpdateAsync(SelectedApp!, progress, cancellationToken);

            await _launcherService.LaunchAppAsync(SelectedApp!, progress, cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = _localizer["AppUpdateErrorMessageBoxTitle", SelectedApp!.Manifest.DisplayName]
            };

            await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
        } finally {
            vm.OkCommand.Execute(null);
            await dialogTask;
        }

        await LoadApps(cancellationToken);
    }

    [RelayCommand]
    private Task OpenAppsFolder() => OpenFolderAsync(_pathService.AppPath);

    [RelayCommand]
    private Task OpenDownloadsFolder() => OpenFolderAsync(_pathService.DownloadPath);

    [RelayCommand]
    private async Task OpenSettings() {
        var vm = _dialogService.CreateViewModel<SettingsViewModel>();
        await _dialogService.ShowDialogAsync(this, vm);
    }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task RemoveApp(CancellationToken cancellationToken) {
        var removeAppMessageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = _localizer["RemoveAppMessageBoxTitle", SelectedApp!.Manifest.DisplayName],
            Content = _localizer["RemoveAppMessageBoxContent", SelectedApp.Manifest.DisplayName]
        };

        var removeAppResult = await _dialogService.ShowMessageBoxAsync(this, removeAppMessageBoxSettings);

        if (removeAppResult is true) {
            if (Directory.Exists(_pathService.GetInstallPath(SelectedApp))) {
                var removeAppDataMessageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.YesNo,
                    DefaultValue = false,
                    Icon = MessageBoxImage.Warning,
                    Title = _localizer["RemoveAppMessageBoxTitle", SelectedApp.Manifest.DisplayName],
                    Content = _localizer["RemoveAppDataMessageBoxContent", SelectedApp.Manifest.DisplayName]
                };

                var removeAppDataResult =
                    await _dialogService.ShowMessageBoxAsync(this, removeAppDataMessageBoxSettings);

                if (removeAppDataResult is true) {
                    _updaterService.DeleteAppData(SelectedApp);
                }
            }

            await _databaseService.RemoveAppAsync(SelectedApp, cancellationToken);
            await LoadApps(cancellationToken);
        }
    }

    [RelayCommand]
    private async Task UpdateRepositories(CancellationToken cancellationToken) {
        // TODO: dont stop updating when only 1 repository throws exceptions
        try {
            await _repositoryService.UpdateRepositoriesAsync(cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                Icon = MessageBoxImage.Error,
                DefaultValue = true,
                Title = _localizer["RepositoryUpdateErrorMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
        }
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await _configService.GetConfigAsync(cancellationToken);
        await LoadApps(cancellationToken);
    }

    private bool CanInteractWithApp() => SelectedApp != null;

    private async Task LoadApps(CancellationToken cancellationToken) {
        Apps.Clear();
        foreach (var app in await _databaseService.GetInstalledAppsAsync(cancellationToken))
            Apps.Add(app);
    }

    private async Task OpenFolderAsync(string path) {
        try {
            _ioDialogService.OpenFolder(path);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                DefaultValue = true,
                Icon = MessageBoxImage.Error,
                Title = _localizer["OpenFolderErrorMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
        }
    }
}
