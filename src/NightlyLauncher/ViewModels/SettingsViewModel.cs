using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class SettingsViewModel : ViewModelBase {
    private readonly IConfigService _configService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;

    public SettingsViewModel(
        IConfigService configService,
        DialogOwner dialogOwner,
        IDialogService dialogService,
        IRepositoryService repositoryService,
        IStringLocalizer<SettingsViewModel> localizer) {
        _configService = configService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _localizer = localizer;

        Title = _localizer["SettingsViewTitle"];
    }

    [ObservableProperty] public partial Config? Config { get; private set; }

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

    [RelayCommand]
    private async Task Save(CancellationToken cancellationToken) =>
        await _configService.UpdateConfigAsync(Config!, cancellationToken);

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await _configService.GetConfigAsync(cancellationToken);
}
