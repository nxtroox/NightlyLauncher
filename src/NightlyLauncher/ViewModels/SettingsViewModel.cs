using System;
using System.Threading;
using System.Threading.Tasks;
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

    public SettingsViewModel(
        IConfigService configService,
        DialogOwner dialogOwner,
        IDialogService dialogService,
        ILauncherService launcherService,
        IStringLocalizer<SettingsViewModel> localizer) {
        _configService = configService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _launcherService = launcherService;
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
        SelectFolderAsync(path => { Config = Config! with { AppPath = path }; });

    [RelayCommand]
    private Task SelectDownloadPath() =>
        SelectFolderAsync(path => { Config = Config! with { DownloadPath = path }; });

    private async Task SelectFolderAsync(Action<string> callback) {
        var settings = new OpenFolderDialogSettings { Title = _localizer["OpenFolderDialogTitle"] };

        var folder = await _dialogService.ShowOpenFolderDialogAsync(_dialogOwner.Window!, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }
}
