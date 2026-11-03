using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.Windows;

internal partial class MainWindow : Window {
    public MainWindow() => InitializeComponent();

    protected override async void OnOpened(EventArgs e) {
        if (DataContext is MainViewModel viewModel)
            await viewModel.InitializeAsync();

        base.OnOpened(e);
    }

    private void AppListBox_DoubleTapped(object? sender, TappedEventArgs e) {
        var clickedItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();

        if (clickedItem is not null && DataContext is MainViewModel viewModel &&
            viewModel.LaunchAppCommand.CanExecute(null))
            viewModel.LaunchAppCommand.Execute(null);
    }
}
