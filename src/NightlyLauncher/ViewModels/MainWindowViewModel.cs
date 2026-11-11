using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase {
    private readonly AppBrowserViewModel _appBrowserViewModel;
    private readonly LibraryViewModel _libraryViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public MainWindowViewModel(AppBrowserViewModel appBrowserViewModel, LibraryViewModel libraryViewModel,
        SettingsViewModel settingsViewModel,
        DialogOwner dialogOwner) {
        _appBrowserViewModel = appBrowserViewModel;
        _libraryViewModel = libraryViewModel;
        _settingsViewModel = settingsViewModel;

        dialogOwner.Window = this;

        CurrentPage = _libraryViewModel;
        Title = _libraryViewModel.Title;
    }

    [ObservableProperty] public partial ViewModelBase CurrentPage { get; set; }

    public new string? Title {
        get;
        set => field = value + " - NightlyLauncher";
    }

    [RelayCommand]
    private void Navigate(string page) {
        ViewModelBase vm = page switch {
            "AppBrowser" => _appBrowserViewModel,
            "Library" => _libraryViewModel,
            "Settings" => _settingsViewModel,
            _ => throw new NotImplementedException()
        };

        CurrentPage = vm;
        Title = vm.Title;
    }
}
