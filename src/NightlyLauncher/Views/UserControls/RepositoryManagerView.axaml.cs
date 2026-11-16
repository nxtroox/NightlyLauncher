using Avalonia.Controls;
using Avalonia.Interactivity;
using NightlyLauncher.ViewModels;

namespace NightlyLauncher.Views.UserControls;

internal sealed partial class RepositoryManagerView : UserControl {
    public RepositoryManagerView() => InitializeComponent();

    protected override async void OnLoaded(RoutedEventArgs e) {
        if (DataContext is RepositoryManagerViewModel viewModel)
            await viewModel.InitializeAsync();

        base.OnLoaded(e);
    }
}
