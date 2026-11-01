using HanumanInstitute.MvvmDialogs.Avalonia;
using NightlyLauncher.ViewModels;
using NightlyLauncher.Views.Windows;

namespace NightlyLauncher;

public sealed class ViewLocator : StrongViewLocator {
    public ViewLocator() {
        Register<AppBrowserViewModel, AppBrowserWindow>();
        Register<EditViewModel, EditWindow>();
        Register<InputViewModel, InputWindow>();
        Register<MainViewModel, MainWindow>();
        Register<SelectionViewModel, SelectionWindow>();
        Register<SettingsViewModel, SettingsWindow>();
        Register<UpdaterViewModel, UpdaterWindow>();
    }
}
