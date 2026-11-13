using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal abstract partial class GenericDialogViewModelBase : ObservableObject {
    [ObservableProperty] public partial string? Content { get; set; }
}
