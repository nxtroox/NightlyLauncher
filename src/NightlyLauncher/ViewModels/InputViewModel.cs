using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal partial class InputViewModel : GenericDialogViewModelBase {
    [ObservableProperty] public partial string? Text { get; set; }
    [ObservableProperty] public partial string? TextHint { get; set; }
}
