using System;
using Avalonia.Controls;
using Avalonia.Input;
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
        if (DataContext is MainViewModel mainViewModel)
            mainViewModel.LaunchAppCommand.Execute(null);
    }
}
