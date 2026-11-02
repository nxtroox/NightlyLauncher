using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.ViewModels;

internal partial class EditViewModel : DialogViewModelBase {
    public EditViewModel(IStringLocalizer<EditViewModel> localizer) => Title = localizer["EditWindowTitle"];

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

    partial void OnSelectedAppChanged(InstalledApp? value) => OnPropertyChanged(nameof(SelectedInstallOption));

    protected override bool CanSubmit() => true;
}
