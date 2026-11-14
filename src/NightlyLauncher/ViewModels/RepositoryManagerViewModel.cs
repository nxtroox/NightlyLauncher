using System;
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

internal sealed partial class RepositoryManagerViewModel : ViewModelBase {
    private readonly IConfigService _configService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<RepositoryManagerViewModel> _localizer;

    private readonly IRepositoryService _repositoryService;

    public RepositoryManagerViewModel(IConfigService configService, DialogOwner dialogOwner,
        IDialogService dialogService,
        IRepositoryService repositoryService, IStringLocalizer<RepositoryManagerViewModel> localizer) {
        _configService = configService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = _localizer["RepositoryManagerViewTitle"];
    }

    [ObservableProperty] public partial Config? Config { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRepositoryCommand))]
    public partial Repository? SelectedRepository { get; set; }

    private bool CanModifyRepository => SelectedRepository is not null;

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await _configService.GetConfigAsync(cancellationToken);

    [RelayCommand]
    private async Task AddRepository(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<InputDialogViewModel>();

        vm.TextHint = _localizer["UrlInputContentDialogTextHint"];

        var contentDialogSettings = new ContentDialogSettings {
            CloseButtonText = _localizer["UrlInputContentDialogCloseButtonText"],
            Content = vm,
            DefaultButton = FAContentDialogButton.Primary,
            PrimaryButtonText = _localizer["UrlInputContentDialogPrimaryButtonText"],
            Title = _localizer["UrlInputContentDialogTitle"]
        };

        var result = await _dialogService.ShowContentDialogAsync(_dialogOwner.Window!, contentDialogSettings);

        if (result is FAContentDialogResult.Primary) {
            try {
                await _repositoryService.AddRepositoryAsync(new Uri(vm.Text!), cancellationToken);
                Config = await _configService.GetConfigAsync(cancellationToken);
            } catch (Exception ex) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.Ok,
                    Content = ex.Message,
                    DefaultValue = true,
                    Icon = MessageBoxImage.Error,
                    Title = _localizer["AddRepositoryErrorMessageBoxTitle"]
                };

                await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, messageBoxSettings);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyRepository))]
    private async Task RemoveRepository(CancellationToken cancellationToken) {
        var confirmationMessageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            Content = _localizer["RemoveRepositoryConfirmationMessageBoxContent"],
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = _localizer["RemoveRepositoryConfirmationMessageBoxTitle", SelectedRepository!.DisplayName]
        };

        var result = await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, confirmationMessageBoxSettings);

        if (result is true) {
            var repositoryToRemove = SelectedRepository!;

            Config = Config! with {
                Repositories = Config.Repositories?.Where(repository => repositoryToRemove.Guid != repository.Guid)
                    .ToList()
            };

            try {
                await _repositoryService.RemoveRepositoryAsync(repositoryToRemove, cancellationToken);
                SelectedRepository = null;
            } catch (Exception ex) {
                var errorMessageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.Ok,
                    Content = ex.Message,
                    DefaultValue = true,
                    Icon = MessageBoxImage.Error,
                    Title = _localizer["RemoveRepositoryErrorMessageBoxTitle", repositoryToRemove.DisplayName]
                };

                await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window!, errorMessageBoxSettings);
                Config = await _configService.GetConfigAsync(cancellationToken);
            }
        }
    }
}
