using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal sealed partial class InputDialogViewModel : ObservableObject {
    [ObservableProperty] public partial string? Text { get; set; }
    [ObservableProperty] public partial string? TextHint { get; set; }
}
