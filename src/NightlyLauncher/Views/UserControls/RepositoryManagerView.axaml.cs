using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.UserControls;

internal sealed partial class RepositoryManagerView : UserControl {
    public RepositoryManagerView() => InitializeComponent();

    protected override async void OnLoaded(RoutedEventArgs e) {
        if (DataContext is RepositoryManagerViewModel viewModel)
            await viewModel.InitializeAsync();

        base.OnLoaded(e);
    }

    private void RepositoryListBox_DoubleTapped(object? sender, TappedEventArgs e) {
        var clickedItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();

        if (clickedItem is not null && DataContext is RepositoryManagerViewModel viewModel &&
            viewModel.OpenRepositoryCommand.CanExecute(null))
            viewModel.OpenRepositoryCommand.Execute(null);
    }
}
