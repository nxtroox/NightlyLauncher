using System;
using Avalonia.Controls;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.Windows;

internal partial class SettingsWindow : Window {
    public SettingsWindow() => InitializeComponent();

    protected override async void OnOpened(EventArgs e) {
        if (DataContext is SettingsViewModel viewModel) {
            await viewModel.InitializeAsync();
            viewModel.OnRequestClose += (_, _) => Close(viewModel.DialogResult);
        }

        base.OnOpened(e);
    }
}
