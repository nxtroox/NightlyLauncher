using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal partial class SelectionViewModel : GenericDialogViewModelBase {
    public ObservableCollection<object> Items { get; } = [];
    [ObservableProperty] public partial object? SelectedItem { get; set; }
}
