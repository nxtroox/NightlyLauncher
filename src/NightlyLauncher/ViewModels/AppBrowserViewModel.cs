using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class AppBrowserViewModel : DialogViewModelBase {
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<AppBrowserViewModel> _localizer;
    private readonly IRepositoryService _repositoryService;

    public AppBrowserViewModel(IDialogService dialogService, IRepositoryService repositoryService,
        IStringLocalizer<AppBrowserViewModel> localizer) {
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = localizer["AppBrowserWindowTitle"];
        SearchPlaceholderText = localizer["AppBrowserWindowContent"];

        Apps.CollectionChanged += (_, _) => ApplySearchFilter();
        ApplySearchFilter();
    }

    public ObservableCollection<AppManifest> Apps { get; } = [];
    public ObservableCollection<AppManifest> FilteredApps { get; } = [];

    [ObservableProperty] public partial AppManifest? SelectedApp { get; set; }
    [ObservableProperty] public partial string? SearchText { get; set; }
    [ObservableProperty] public partial string? SearchPlaceholderText { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    public partial Guid? SelectedInstallOptionGuid { get; private set; }

    [RelayCommand]
    private async Task AddApp() {
        var compatibleOptions = SelectedApp!.InstallOptions.Where(option =>
                _repositoryService.CheckAppCompatibility(option.Platform.OperatingSystem, option.Platform.Architecture))
            .ToList();

        if (compatibleOptions.Count is 1) {
            SelectedInstallOptionGuid = compatibleOptions[0].Guid;
            OkCommand.Execute(null);
            return;
        }

        var vm = _dialogService.CreateViewModel<SelectionViewModel>();

        vm.Title = _localizer["SelectInstallOptionDialogTitle"];
        vm.Content = _localizer["SelectInstallOptionDialogContent"];

        foreach (var option in compatibleOptions)
            vm.Items.Add(option);

        if (await _dialogService.ShowDialogAsync(this, vm) is true && vm.SelectedItem is InstallOption installOption) {
            SelectedInstallOptionGuid = installOption.Guid;
            OkCommand.Execute(null);
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

    protected override bool CanSubmit() => SelectedInstallOptionGuid is not null;
}
