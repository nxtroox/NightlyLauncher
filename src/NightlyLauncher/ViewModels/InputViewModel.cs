using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal partial class InputViewModel : DialogViewModelBase {
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    public partial string? Text { get; set; }

    [ObservableProperty] public partial string? TextHint { get; set; }

    protected override bool CanSubmit() => !string.IsNullOrWhiteSpace(Text);
}
