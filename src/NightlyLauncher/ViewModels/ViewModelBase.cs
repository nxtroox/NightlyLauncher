using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal abstract partial class ViewModelBase : ObservableObject {
    [ObservableProperty] public virtual partial string? Title { get; set; }
}
