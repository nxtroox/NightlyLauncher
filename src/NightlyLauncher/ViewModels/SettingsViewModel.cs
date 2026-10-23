using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class SettingsViewModel(IConfigService configService, IDialogService dialogService)
    : ViewModelBase, IModalDialogViewModel {
    [ObservableProperty] public partial Config? Config { get; private set; }

    public bool? DialogResult { get; private set; }

    internal event EventHandler? OnRequestClose;

    [RelayCommand]
    private Task SelectAppPath() =>
        SelectFolderAsync(path => { Config = Config! with { AppPath = path }; });

    [RelayCommand]
    private Task SelectDownloadPath() =>
        SelectFolderAsync(path => { Config = Config! with { DownloadPath = path }; });

    private async Task SelectFolderAsync(Action<string> callback) {
        var settings = new OpenFolderDialogSettings { Title = "Select Folder" };

        var folder = await dialogService.ShowOpenFolderDialogAsync(this, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }

    [RelayCommand]
    private async Task SaveConfig(CancellationToken cancellationToken) {
        await configService.UpdateConfigAsync(Config!, cancellationToken);
        DialogResult = true;
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() {
        DialogResult = false;
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await configService.GetConfigAsync(cancellationToken);
}
