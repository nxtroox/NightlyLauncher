using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Core.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal sealed partial class AppBrowserViewModel : ViewModelBase {
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

        Apps.CollectionChanged += (_, _) => ApplySearchFilter();
        ApplySearchFilter();
    }

    private ObservableCollection<AppManifest> Apps { get; } = [];
    public ObservableCollection<AppManifest> FilteredApps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAppCommand))]
    public partial AppManifest? SelectedManifest { get; set; }

    [ObservableProperty] public partial string? SearchText { get; set; }

    private Guid? SelectedInstallOptionGuid { get; set; }

    [RelayCommand(CanExecute = nameof(CanInteractWithApp))]
    private async Task AddApp(CancellationToken cancellationToken) {
        var compatibleOptions = SelectedManifest!.InstallOptions.Where(option =>
                _repositoryService.CheckAppCompatibility(option.Platform.OsType, option.Platform.Architecture))
            .ToList();

        if (compatibleOptions.Count is 1)
            SelectedInstallOptionGuid = compatibleOptions.First().Guid;
        else {
            var vm = _dialogService.CreateViewModel<SelectionViewModel>();
            vm.Content = _localizer["SelectInstallOptionContentDialogContent"];
            compatibleOptions.ForEach(vm.Items.Add);

            var contentDialogSettings = new ContentDialogSettings {
                CloseButtonText = _localizer["SelectInstallOptionContentCloseButtonText"],
                Content = vm,
                DefaultButton = FAContentDialogButton.Primary,
                PrimaryButtonText = _localizer["SelectInstallOptionContentDialogPrimaryButtonText"],
                Title = _localizer["SelectInstallOptionContentDialogTitle"]
            };

            if (await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings) is
                    FAContentDialogResult.Primary && vm.SelectedItem is InstallOption installOption)
                SelectedInstallOptionGuid = installOption.Guid;
        }

        if (SelectedInstallOptionGuid is null) return;

        try {
            await InstallAppAsync(cancellationToken);
        } catch (Exception ex) {
            var messageBoxSettings = new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = ex.Message,
                DefaultValue = true,
                Icon = MessageBoxImage.Error,
                Title = _localizer["AddAppErrorMessageBoxTitle", SelectedManifest.DisplayName]
            };

            await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window, messageBoxSettings);
        }
    }

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnSearchTextChanged(string? value) => ApplySearchFilter();

    private void ApplySearchFilter() {
        var query = SearchText?.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? Apps
            : Apps.Where(app => app.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));

        FilteredApps.Clear();
        foreach (var app in matches)
            FilteredApps.Add(app);

        if (SelectedManifest is not null && !FilteredApps.Contains(SelectedManifest))
            SelectedManifest = null;
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

    private bool CanInteractWithApp() => SelectedManifest is not null;

    private Task LoadAppsAsync(CancellationToken cancellationToken) {
        Apps.Clear();
        return InitializeAsync(cancellationToken);
    }

    private async Task InstallAppAsync(CancellationToken cancellationToken) {
        var option =
            SelectedManifest!.InstallOptions.FirstOrDefault(option => option.Guid == SelectedInstallOptionGuid);

        var app = new InstalledApp(SelectedManifest!,
            SelectedInstallOptionGuid ??
            throw new InvalidOperationException(_localizer["NoInstallOptionSelectedExceptionMessage"]),
            option!.AppOptions);

        await _databaseService.UpdateAppAsync(app, cancellationToken);
        await LoadAppsAsync(cancellationToken);
    }
}
