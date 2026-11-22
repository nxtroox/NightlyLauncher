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

    private IReadOnlyList<AppManifest> _apps = [];

    public AppBrowserViewModel(IDatabaseService databaseService, DialogOwner dialogOwner, IDialogService dialogService,
        IRepositoryService repositoryService, IStringLocalizer<AppBrowserViewModel> localizer) {
        _databaseService = databaseService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = localizer["AppBrowserViewTitle"];
    }

    public ObservableCollection<AppManifest> FilteredApps { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAppCommand))]
    public partial AppManifest? SelectedManifest { get; set; }

    [ObservableProperty] public partial string? SearchText { get; set; }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) {
        var manifests = await _repositoryService.GetAvailableAppsAsync(cancellationToken);
        var installedApps = await _databaseService.GetInstalledAppsAsync(cancellationToken);

        var installedGuids = installedApps.Select(app => app.Manifest.Guid).ToHashSet();
        _apps = [.. manifests.Where(manifest => !installedGuids.Contains(manifest.Guid))];

        if (_apps.Count is 0) {
            await ShowMessageBoxAsync(
                _localizer["NoAppsAvailableMessageBoxTitle"],
                _localizer["NoAppsAvailableMessageBoxContent"],
                MessageBoxImage.Information);
            return;
        }

        ApplySearchFilter();
    }

    partial void OnSearchTextChanged(string? value) {
        if (_apps.Count is not 0)
            ApplySearchFilter();
    }

    private void ApplySearchFilter() {
        var query = SearchText?.Trim();
        var matches = string.IsNullOrEmpty(query)
            ? _apps
            : _apps.Where(app => app.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));

        FilteredApps.Clear();
        foreach (var app in matches)
            FilteredApps.Add(app);

        if (SelectedManifest is not null && !FilteredApps.Contains(SelectedManifest))
            SelectedManifest = null;
    }

    private bool CanAddApp() => SelectedManifest is not null;

    [RelayCommand(CanExecute = nameof(CanAddApp))]
    private async Task AddApp(CancellationToken cancellationToken) {
        var manifest = SelectedManifest!;

        var compatibleOptions = manifest.InstallOptions
            .Where(option => _repositoryService.CheckAppCompatibility(option.Platform.OsType,
                option.Platform.Architecture))
            .ToList();

        var installOption = compatibleOptions switch {
            [] => null, // this is technically not possible, but better be safe than sorry I guess
            [var single] => single,
            _ => await SelectInstallOptionAsync(compatibleOptions)
        };

        if (installOption is null) return;

        try {
            var app = new InstalledApp(manifest, installOption.Guid, installOption.AppOptions);
            await _databaseService.UpdateAppAsync(app, cancellationToken);
            await InitializeAsync(cancellationToken);
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            await ShowMessageBoxAsync(
                _localizer["AddAppErrorMessageBoxTitle", manifest.DisplayName],
                ex.Message,
                MessageBoxImage.Error);
        }
    }

    private async Task<InstallOption?> SelectInstallOptionAsync(IEnumerable<InstallOption> options) {
        var vm = _dialogService.CreateViewModel<SelectionViewModel>();
        vm.Content = _localizer["SelectInstallOptionContentDialogContent"];
        foreach (var option in options)
            vm.Items.Add(option);

        var settings = new ContentDialogSettings {
            CloseButtonText = _localizer["SelectInstallOptionContentCloseButtonText"],
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = _localizer["SelectInstallOptionContentDialogPrimaryButtonText"],
            Title = _localizer["SelectInstallOptionContentDialogTitle"]
        };

        var result = await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, settings);
        return result is FAContentDialogResult.Primary ? vm.SelectedItem as InstallOption : null;
    }

    private Task<bool?> ShowMessageBoxAsync(string title, string content, MessageBoxImage icon) =>
        _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!,
            new MessageBoxSettings {
                Button = MessageBoxButton.Ok,
                Content = content,
                DefaultValue = true,
                Icon = icon,
                Title = title
            });
}
