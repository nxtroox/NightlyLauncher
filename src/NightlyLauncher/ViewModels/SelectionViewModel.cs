using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NightlyLauncher.ViewModels;

internal partial class SelectionViewModel : PromptDialogViewModelBase {
    public ObservableCollection<object> Items { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    public partial object? SelectedItem { get; set; }

    protected override bool CanSubmit() => SelectedItem is not null;
}
