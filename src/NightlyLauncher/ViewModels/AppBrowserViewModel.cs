using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class AppBrowserViewModel : ViewModelBase {
    private readonly IDatabaseService _databaseService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<AppBrowserViewModel> _localizer;
    private readonly IRepositoryService _repositoryService;

    public AppBrowserViewModel(IDatabaseService databaseService, DialogOwner dialogOwner, IDialogService dialogService,
        IRepositoryService repositoryService,
        IStringLocalizer<AppBrowserViewModel> localizer) {
        _databaseService = databaseService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = localizer["AppBrowserViewTitle"];
        SearchPlaceholderText = localizer["AppBrowserSearchPlaceholderText"];

        Apps.CollectionChanged += (_, _) => ApplySearchFilter();
        ApplySearchFilter();
    }

    private ObservableCollection<AppManifest> Apps { get; } = [];
    public ObservableCollection<AppManifest> FilteredApps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAppCommand))]
    public partial AppManifest? SelectedApp { get; set; }

    [ObservableProperty] public partial Guid? SelectedInstallOptionGuid { get; private set; }

    [ObservableProperty] public partial string? SearchText { get; set; }
    [ObservableProperty] public partial string? SearchPlaceholderText { get; set; }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task AddApp(CancellationToken cancellationToken) {
        var compatibleOptions = SelectedApp!.InstallOptions.Where(option =>
                _repositoryService.CheckAppCompatibility(option.Platform.OperatingSystem, option.Platform.Architecture))
            .ToList();

        if (compatibleOptions.Count is 1) {
            SelectedInstallOptionGuid = compatibleOptions[0].Guid;
            await InstallAppAsync(cancellationToken);
            return;
        }

        var vm = _dialogService.CreateViewModel<SelectionViewModel>();
        vm.Content = _localizer["SelectInstallOptionDialogContent"];

        foreach (var option in compatibleOptions)
            vm.Items.Add(option);

        var contentDialogSettings = new ContentDialogSettings {
            CloseButtonText = "Close",
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = "Select",
            Title = _localizer["SelectInstallOptionDialogTitle"]
        };

        if (await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings) is
                FAContentDialogResult.Primary && vm.SelectedItem is InstallOption installOption) {
            SelectedInstallOptionGuid = installOption.Guid;
            await InstallAppAsync(cancellationToken);
        }
    }

    partial void OnSearchTextChanged(string? value) => ApplySearchFilter();

    private void ApplySearchFilter() {
        var query = SearchText?.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? Apps
            : Apps.Where(app => app.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));

        FilteredApps.Clear();
        foreach (var app in matches)
            FilteredApps.Add(app);

        if (SelectedApp is not null && !FilteredApps.Contains(SelectedApp))
            SelectedApp = null;
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        var manifests = await _repositoryService.GetAvailableAppsAsync(cancellationToken);
        var installedApps = await _databaseService.GetInstalledAppsAsync(cancellationToken);

        var apps = manifests
            .Where(manifest => installedApps.All(m => m.Manifest.Guid != manifest.Guid))
            .ToList();

        if (apps.Count is 0) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = _localizer["NoAppsAvailableMessageBoxContent"],
                DefaultValue = true,
                Icon = MessageBoxImage.Information,
                Title = _localizer["NoAppsAvailableMessageBoxTitle"]
            };

            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings);
            return;
        }

        foreach (var app in apps)
            Apps.Add(app);
    }

    private bool CanInteractWithApp() => SelectedApp is not null;

    private async Task LoadAppsAsync(CancellationToken cancellationToken) {
        Apps.Clear();
        await InitializeAsync(cancellationToken);
    }

    private async Task InstallAppAsync(CancellationToken cancellationToken) {
        var app = new InstalledApp(SelectedApp!,
            SelectedInstallOptionGuid ??
            throw new InvalidOperationException(_localizer["NoInstallOptionSelectedExceptionMessage"]));

        await _databaseService.UpdateAppAsync(app, cancellationToken);
        await LoadAppsAsync(cancellationToken);
    }
}
