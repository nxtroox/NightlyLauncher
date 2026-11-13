using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase {
    private readonly IStringLocalizer<MainWindowViewModel> _localizer;
    private readonly Func<Type, ViewModelBase> _viewModelFactory;

    public MainWindowViewModel(Func<Type, ViewModelBase> viewModelFactory,
        IStringLocalizer<MainWindowViewModel> localizer, DialogOwner dialogOwner) {
        _viewModelFactory = viewModelFactory;
        _localizer = localizer;
        dialogOwner.Window = this;

        MenuItems = [
            new NavigationItem(_localizer["LibraryTitle"], Symbol.Library, typeof(LibraryViewModel)),
            new NavigationItem(_localizer["AppBrowserTitle"], Symbol.BuildingShop, typeof(AppBrowserViewModel))
        ];

        FooterItems = [
            new NavigationItem(_localizer["SettingsTitle"], Symbol.Settings, typeof(SettingsViewModel))
        ];

        SelectedItem = MenuItems[0];
    }

    [ObservableProperty] public partial NavigationItem? SelectedItem { get; set; }
    [ObservableProperty] public partial ViewModelBase? CurrentPage { get; set; }

    public ObservableCollection<NavigationItem> MenuItems { get; }

    public ObservableCollection<NavigationItem> FooterItems { get; }

    public new string? Title {
        get;
        set {
            field = $"{value} - {_localizer["AppName"]}";
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
