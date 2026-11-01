using Avalonia.Controls;
using Avalonia.Input;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.Windows;

internal partial class AppBrowserWindow : Window {
    public AppBrowserWindow() => InitializeComponent();

    private void AppListBox_DoubleTapped(object? sender, TappedEventArgs e) {
        if (DataContext is AppBrowserViewModel viewModel)
            viewModel.AddAppCommand.Execute(null);
    }
}
