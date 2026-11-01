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

        Title = localizer["Title"];
        Content = localizer["Content"];
    }

    public ObservableCollection<AppManifest> Apps { get; set; } = [];

    [ObservableProperty] public partial AppManifest? SelectedApp { get; set; }
    [ObservableProperty] public partial string? SearchBoxInput { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    public partial Guid? SelectedInstallOptionGuid { get; private set; }

    [RelayCommand]
    private async Task AddApp() {
        var compatibleApps = SelectedApp!.InstallOptions.Where(option =>
                _repositoryService.CheckAppCompatibility(option.Platform.OperatingSystem, option.Platform.Architecture))
            .ToList();

        if (compatibleApps.Count is 1) {
            SelectedInstallOptionGuid = compatibleApps[0].Guid;
            OkCommand.Execute(null);
            return;
        }

        var vm = _dialogService.CreateViewModel<SelectionViewModel>();

        vm.Title = _localizer["SelectInstallOptionTitle"];
        vm.Content = _localizer["SelectInstallOptionContent"];

        foreach (var option in SelectedApp!.InstallOptions) {
            if (_repositoryService.CheckAppCompatibility(option.Platform.OperatingSystem, option.Platform.Architecture))
                vm.Items.Add(option);
        }

        if (await _dialogService.ShowDialogAsync(this, vm) is true && vm.SelectedItem is InstallOption installOption) {
            SelectedInstallOptionGuid = installOption.Guid;
            OkCommand.Execute(null);
        }
    }

    protected override bool CanSubmit() => SelectedInstallOptionGuid is not null;
}
