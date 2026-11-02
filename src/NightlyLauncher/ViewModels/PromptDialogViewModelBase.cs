using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal abstract partial class PromptDialogViewModelBase : DialogViewModelBase {
    [ObservableProperty] public partial string? Content { get; set; }
}
