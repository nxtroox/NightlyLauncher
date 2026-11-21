using CommunityToolkit.Mvvm.ComponentModel;
using NightlyLauncher.Core.Models;

namespace NightlyLauncher.ViewModels;

internal sealed partial class EditDialogViewModel : ObservableObject {
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedInstallOption))]
    public partial InstalledApp? SelectedApp { get; set; }

    public InstallOption? SelectedInstallOption {
        get => SelectedApp?.Manifest.InstallOptions.FirstOrDefault(option =>
            option.Guid == SelectedApp.InstallOptionGuid);

        set {
            if (SelectedApp is not { } app ||
                value is null || app.Manifest.InstallOptions.All(option => option.Guid != value.Guid) ||
                app.InstallOptionGuid == value.Guid) {
                return;
            }

            SelectedApp = app with { InstallOptionGuid = value.Guid };
        }
    }
}
