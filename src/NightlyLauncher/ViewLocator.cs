using HanumanInstitute.MvvmDialogs.Avalonia;
using NightlyLauncher.ViewModels;
using NightlyLauncher.Views.Windows;

namespace NightlyLauncher;

public sealed class ViewLocator : StrongViewLocator {
    public ViewLocator() {
        Register<MainViewModel, MainWindow>();
        Register<SettingsViewModel, SettingsWindow>();
        Register<UpdaterViewModel, UpdaterWindow>();
    }
}
