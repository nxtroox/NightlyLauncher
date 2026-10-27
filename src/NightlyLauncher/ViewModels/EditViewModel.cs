using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using NightlyLauncher.Models;

namespace NightlyLauncher.ViewModels;

internal partial class EditViewModel : ViewModelBase, IModalDialogViewModel, ICloseable {
    [ObservableProperty] public partial InstalledApp? SelectedApp { get; set; }

    public InstallOption? SelectedInstallOption {
        get => SelectedApp?.Manifest.InstallOptions.FirstOrDefault(option =>
            option.Guid == SelectedApp.SelectedInstallOptionGuid);

        set {
            if (SelectedApp is not { } app ||
                value is null ||
                app.SelectedInstallOptionGuid == value.Guid) {
                return;
            }

            SelectedApp = app with { SelectedInstallOptionGuid = value.Guid };
        }
    }

    public event EventHandler? RequestClose;
    public bool? DialogResult { get; private set; }

    partial void OnSelectedAppChanged(InstalledApp? value) => OnPropertyChanged(nameof(SelectedInstallOption));

    [RelayCommand]
    private void Save() {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
