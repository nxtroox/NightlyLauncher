using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;

namespace NightlyLauncher.ViewModels;

internal partial class EditDialogViewModel(IStringLocalizer<EditDialogViewModel> localizer) : ViewModelBase {
    public new string Title => localizer["EditDialogViewTitle", SelectedApp?.Manifest.DisplayName ?? string.Empty];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedInstallOption))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial InstalledApp? SelectedApp { get; set; }

    public InstallOption? SelectedInstallOption {
        get => SelectedApp?.Manifest.InstallOptions.FirstOrDefault(option =>
            option.Guid == SelectedApp.SelectedInstallOptionGuid);

        set {
            if (SelectedApp is not { } app ||
                value is null || app.Manifest.InstallOptions.All(option => option.Guid != value.Guid) ||
                app.SelectedInstallOptionGuid == value.Guid) {
                return;
            }

            SelectedApp = app with { SelectedInstallOptionGuid = value.Guid };
        }
    }
}
