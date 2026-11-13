using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase {
    private readonly Func<Type, ViewModelBase> _viewModelFactory;

    public MainWindowViewModel(Func<Type, ViewModelBase> viewModelFactory, DialogOwner dialogOwner) {
        _viewModelFactory = viewModelFactory;
        dialogOwner.Window = this;

        SelectedItem = MenuItems[0];
    }

    [ObservableProperty] public partial NavigationItem? SelectedItem { get; set; }
    [ObservableProperty] public partial ViewModelBase? CurrentPage { get; set; }

    public ObservableCollection<NavigationItem> MenuItems { get; } = [
        new("Library", Symbol.Library, typeof(LibraryViewModel)),
        new("Browse Apps", Symbol.BuildingShop, typeof(AppBrowserViewModel))
    ];

    public ObservableCollection<NavigationItem> FooterItems { get; } = [
        new("Settings", Symbol.Settings, typeof(SettingsViewModel))
    ];

    public new string? Title {
        get;
        set {
            field = value + " - NightlyLauncher";
            OnPropertyChanged();
        }
    }

    partial void OnSelectedItemChanged(NavigationItem? value) {
        if (value is null) return;

        var viewModel = _viewModelFactory(value.TargetViewModelType);
        CurrentPage = viewModel;
        Title = viewModel.Title;
    }
}
