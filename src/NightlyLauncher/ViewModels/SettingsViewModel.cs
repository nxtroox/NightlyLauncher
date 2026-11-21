using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal sealed partial class SettingsViewModel : ViewModelBase {
    private readonly IConfigService _configService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly ILauncherService _launcherService;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;
    private readonly IPathService _pathService;

    public SettingsViewModel(
        IConfigService configService,
        DialogOwner dialogOwner,
        IDialogService dialogService,
        ILauncherService launcherService,
        IPathService pathService,
        IStringLocalizer<SettingsViewModel> localizer) {
        _configService = configService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _launcherService = launcherService;
        _pathService = pathService;
        _localizer = localizer;

        Title = _localizer["SettingsViewTitle"];
        Config = _configService.Config;
    }

    [ObservableProperty] public partial Config? Config { get; private set; }

    [RelayCommand]
    private Task<FAContentDialogResult> OpenAboutDialog() {
        var vm = _dialogService.CreateViewModel<AboutDialogViewModel>();

        var contentDialogSettings = new ContentDialogSettings {
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = _localizer["AboutContentDialogPrimaryButtonText"],
            Title = vm.Title ?? string.Empty
        };

        return _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings);
    }

    [RelayCommand]
    private async Task ChangeAppPath() {
        var result = await ShowChangePathDialogAsync("AppsFolderName", new Config().AppPath);

        if (!string.IsNullOrWhiteSpace(result))
            Config = Config! with { AppPath = result };
    }

    [RelayCommand]
    private async Task ChangeDownloadPath() {
        var result = await ShowChangePathDialogAsync("DownloadsFolderName", new Config().DownloadPath);

        if (!string.IsNullOrWhiteSpace(result))
            Config = Config! with { DownloadPath = result };
    }

    [RelayCommand]
    private Task<bool> OpenIssuesPage() =>
        _launcherService.OpenUriAsync(new Uri("https://github.com/nxtroox/NightlyLauncher/issues/new"));

    [RelayCommand]
    private Task<bool> OpenSourceCodePage() =>
        _launcherService.OpenUriAsync(new Uri("https://github.com/nxtroox/NightlyLauncher"));

    [RelayCommand]
    private Task Save(CancellationToken cancellationToken) =>
        _configService.UpdateAsync(Config!, cancellationToken);

    [RelayCommand]
    private Task SelectAppPath() =>
        SelectFolderAsync(path => { Config = Config! with { AppPath = path }; }, "AppsFolderName");

    [RelayCommand]
    private Task SelectDownloadPath() =>
        SelectFolderAsync(path => { Config = Config! with { DownloadPath = path }; }, "DownloadsFolderName");

    [RelayCommand]
    private Task<bool> OpenDownloadsFolder() =>
        _launcherService.OpenFolderAsync(_pathService.DownloadPath);

    [RelayCommand]
    private Task<bool> OpenAppsFolder() =>
        _launcherService.OpenFolderAsync(_pathService.AppPath);

    private async Task<string?> ShowChangePathDialogAsync(string folderNameResource, string defaultPath) {
        var vm = _dialogService.CreateViewModel<InputDialogViewModel>();

        vm.TextHint = _localizer[folderNameResource];

        var contentDialogSettings = new ContentDialogSettings {
            CloseButtonText = _localizer["ChangePathDialogCloseButtonText"],
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = _localizer["ChangePathDialogPrimaryButtonText"],
            SecondaryButtonText = _localizer["ChangePathDialogSecondaryButtonText"],
            Title = _localizer["ChangePathDialogTitle", _localizer[folderNameResource]]
        };

        var result = await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings);

        if (result is FAContentDialogResult.Secondary)
            return defaultPath;

        return result is FAContentDialogResult.Primary ? vm.Text : null;
    }

    private async Task SelectFolderAsync(Action<string> callback, string folderResourceName) {
        var settings =
            new OpenFolderDialogSettings {
                Title = _localizer["OpenFolderDialogTitle", _localizer[folderResourceName]]
            };

        var folder = await _dialogService.ShowOpenFolderDialogAsync(_dialogOwner.Window!, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }
}
