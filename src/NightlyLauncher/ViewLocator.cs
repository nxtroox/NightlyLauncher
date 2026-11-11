using HanumanInstitute.MvvmDialogs.Avalonia;
using NightlyLauncher.ViewModels;
using NightlyLauncher.Views.UserControls;
using NightlyLauncher.Views.Windows;

namespace NightlyLauncher;

public sealed class ViewLocator : StrongViewLocator {
    public ViewLocator() {
        Register<AppBrowserViewModel, AppBrowserView>();
        Register<EditViewModel, EditView>();
        Register<InputViewModel, InputWindow>();
        Register<LibraryViewModel, LibraryView>();
        Register<MainWindowViewModel, MainWindow>();
        Register<SelectionViewModel, SelectionWindow>();
        Register<SettingsViewModel, SettingsView>();
        Register<UpdaterViewModel, UpdaterWindow>();
    }
}
