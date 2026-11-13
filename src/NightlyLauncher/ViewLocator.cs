using HanumanInstitute.MvvmDialogs.Avalonia;
using NightlyLauncher.ViewModels;
using NightlyLauncher.Views.UserControls;
using NightlyLauncher.Views.UserControls.ContentDialogs;
using NightlyLauncher.Views.Windows;

namespace NightlyLauncher;

public sealed class ViewLocator : StrongViewLocator {
    public ViewLocator() {
        Register<AppBrowserViewModel, AppBrowserView>();
        Register<EditDialogViewModel, EditDialogView>();
        Register<InputViewModel, InputDialogView>();
        Register<LibraryViewModel, LibraryView>();
        Register<MainWindowViewModel, MainWindow>();
        Register<SelectionViewModel, SelectionDialogView>();
        Register<SettingsViewModel, SettingsView>();
        Register<UpdaterViewModel, UpdaterWindow>();
    }
}
