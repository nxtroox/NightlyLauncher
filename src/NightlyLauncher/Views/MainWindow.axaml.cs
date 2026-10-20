using System;
using Avalonia.Controls;
using Avalonia.Input;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views;

internal partial class MainWindow : Window {
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel mainViewModel) {
        InitializeComponent();
        _viewModel = mainViewModel;
        DataContext = _viewModel;
    }

    protected override async void OnOpened(EventArgs e) {
        await _viewModel.InitializeAsync();
    }

    private void AppListBox_DoubleTapped(object? sender, TappedEventArgs e) {
        if (DataContext is MainViewModel mainViewModel) {
            mainViewModel.LaunchAppCommand.Execute(null);
        }
    }
}
