using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal abstract partial class ViewModelBase : ObservableObject {
    [ObservableProperty] public partial string? Title { get; set; }
}
