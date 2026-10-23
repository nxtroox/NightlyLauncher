using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class SettingsViewModel(IConfigService configService) : ViewModelBase, IModalDialogViewModel {
    [ObservableProperty] public partial Config? Config { get; private set; }

    public bool? DialogResult { get; set; }

    internal event EventHandler? OnRequestClose;

    [RelayCommand]
    private Task SelectFolder() => throw new NotImplementedException();

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
