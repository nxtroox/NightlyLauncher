using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.Windows;

internal partial class MainWindow : FAAppWindow {
    public MainWindow() => InitializeComponent();

    private void MainNav_OnSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e) {
        if (e.SelectedItem is FANavigationViewItem { Tag: string page } && DataContext is MainWindowViewModel viewModel)
            viewModel.NavigateCommand.Execute(page);
    }
}
