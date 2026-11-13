using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.UserControls;

internal partial class AppBrowserView : UserControl {
    public AppBrowserView() => InitializeComponent();

    protected override async void OnLoaded(RoutedEventArgs e) {
        if (DataContext is AppBrowserViewModel viewModel)
            await viewModel.InitializeAsync();

        base.OnLoaded(e);
    }

    private void AppListBox_DoubleTapped(object? sender, TappedEventArgs e) {
        var clickedItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();

        if (clickedItem is not null && DataContext is AppBrowserViewModel viewModel &&
            viewModel.AddAppCommand.CanExecute(null))
            viewModel.AddAppCommand.Execute(null);
    }
}
