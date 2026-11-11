using Avalonia.Controls;
using Avalonia.Interactivity;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.UserControls;

internal partial class SettingsView : UserControl {
    public SettingsView() => InitializeComponent();

    protected override async void OnLoaded(RoutedEventArgs e) {
        if (DataContext is SettingsViewModel viewModel)
            await viewModel.InitializeAsync();

        base.OnLoaded(e);
    }
}
