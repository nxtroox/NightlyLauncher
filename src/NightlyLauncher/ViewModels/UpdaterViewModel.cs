using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;

namespace NightlyLauncher.ViewModels;

internal partial class UpdaterViewModel : DialogViewModelBase {
    public UpdaterViewModel(IStringLocalizer<UpdaterViewModel> localizer) => Title = localizer["UpdaterWindowTitle"];

    [ObservableProperty] public partial string? StatusText { get; set; }
    [ObservableProperty] public partial double ProgressValue { get; set; } = -1;
    [ObservableProperty] public partial bool ProgressIsIndeterminate { get; set; } = true;

    protected override bool CanSubmit() => true;
}
