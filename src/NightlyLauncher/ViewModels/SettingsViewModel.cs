using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Models;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal partial class SettingsViewModel : DialogViewModelBase {
    private readonly IConfigService _configService;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;
    private readonly IRepositoryService _repositoryService;

    public SettingsViewModel(
        IConfigService configService,
        IDialogService dialogService,
        IRepositoryService repositoryService,
        IStringLocalizer<SettingsViewModel> localizer) {
        _configService = configService;
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = _localizer["Title"];
    }

    [ObservableProperty] public partial Config? Config { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRepositoryCommand))]
    public partial Repository? SelectedRepository { get; set; }

    private bool CanModifyRepository => SelectedRepository is not null;

    [RelayCommand]
    private async Task AddRepository(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<InputViewModel>();

        vm.Title = _localizer["UrlInputTitle"];
        vm.Content = _localizer["UrlInputContent"];
        vm.TextHint = _localizer["UrlInputHint"];

        var result = await _dialogService.ShowDialogAsync(this, vm);

        if (result is true) {
            try {
                await _repositoryService.AddRepositoryAsync(new Uri(vm.Text!), cancellationToken);
                Config = await _configService.GetConfigAsync(cancellationToken);
            } catch (Exception ex) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.Ok,
                    Content = ex.Message,
                    DefaultValue = true,
                    Icon = MessageBoxImage.Error,
                    Title = _localizer["AddRepositoryError"]
                };

                await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyRepository))]
    private async Task RemoveRepository(CancellationToken cancellationToken) {
        var messageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            Content = _localizer["RemoveRepositoryConfirmationText", SelectedRepository!.ReadableName],
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = _localizer["RemoveRepositoryConfirmationTitle", SelectedRepository.ReadableName]
        };

        var result = await _dialogService.ShowMessageBoxAsync(this, messageBoxSettings);

        if (result is true) {
            await _repositoryService.RemoveRepositoryAsync(SelectedRepository, cancellationToken);
            Config = await _configService.GetConfigAsync(cancellationToken);
        }
    }

    [RelayCommand]
    private Task SelectAppPath() =>
        SelectFolderAsync(path => { Config = Config! with { AppPath = path }; });

    [RelayCommand]
    private Task SelectDownloadPath() =>
        SelectFolderAsync(path => { Config = Config! with { DownloadPath = path }; });

    private async Task SelectFolderAsync(Action<string> callback) {
        var settings = new OpenFolderDialogSettings { Title = "Select Folder" };

        var folder = await _dialogService.ShowOpenFolderDialogAsync(this, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }

    [RelayCommand]
    private async Task Save(CancellationToken cancellationToken) {
        await _configService.UpdateConfigAsync(Config!, cancellationToken);
        OkCommand.Execute(null);
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await _configService.GetConfigAsync(cancellationToken);

    protected override bool CanSubmit() => true;
}
