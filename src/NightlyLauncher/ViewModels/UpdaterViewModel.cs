using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Localization;

namespace NightlyLauncher.ViewModels;

internal sealed partial class UpdaterViewModel : ViewModelBase, IModalDialogViewModel, ICloseable {
    public UpdaterViewModel(IStringLocalizer<UpdaterViewModel> localizer) => Title = localizer["UpdaterViewTitle"];

    [ObservableProperty] public partial string? StatusText { get; set; }
    [ObservableProperty] public partial double ProgressValue { get; set; } = -1;
    [ObservableProperty] public partial bool ProgressIsIndeterminate { get; set; } = true;

    public event EventHandler? RequestClose;
    public bool? DialogResult { get; private set; }

    internal void Close() {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
