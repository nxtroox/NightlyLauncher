using System;
using System.Collections.Generic;
using System.Linq;
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

internal partial class SettingsViewModel : ViewModelBase {
    private readonly IConfigService _configService;
    private readonly DialogOwner _dialogOwner;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer<SettingsViewModel> _localizer;
    private readonly List<Repository> _repositoriesToRemove = [];
    private readonly IRepositoryService _repositoryService;

    public SettingsViewModel(
        IConfigService configService,
        DialogOwner dialogOwner,
        IDialogService dialogService,
        IRepositoryService repositoryService,
        IStringLocalizer<SettingsViewModel> localizer) {
        _configService = configService;
        _dialogOwner = dialogOwner;
        _dialogService = dialogService;
        _repositoryService = repositoryService;
        _localizer = localizer;

        Title = _localizer["SettingsWindowTitle"];
    }

    [ObservableProperty] public partial Config? Config { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRepositoryCommand))]
    public partial Repository? SelectedRepository { get; set; }

    private bool CanModifyRepository => SelectedRepository is not null;

    [RelayCommand]
    private async Task AddRepository(CancellationToken cancellationToken) {
        var vm = _dialogService.CreateViewModel<InputViewModel>();

        vm.Title = _localizer["AddRepositoryMessageBoxTitle"];
        vm.Content = _localizer["UrlInputMessageBoxContent"];
        vm.TextHint = _localizer["UrlInputMessageBoxTextHint"];

        var result = await _dialogService.ShowDialogAsync(_dialogOwner.Window ?? this, vm);

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
                    Title = _localizer["AddRepositoryMessageBoxTitle"]
                };

                await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window ?? this, messageBoxSettings);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyRepository))]
    private async Task RemoveRepository(CancellationToken cancellationToken) {
        var messageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            Content = _localizer["RemoveRepositoryMessageBoxContent", SelectedRepository!.DisplayName],
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = _localizer["RemoveRepositoryMessageBoxTitle"]
        };

        var result = await _dialogService.ShowMessageBoxAsync(_dialogOwner.Window ?? this, messageBoxSettings);

        if (result is true) {
            var repository = SelectedRepository!;
            _repositoriesToRemove.Add(repository);
            SetConfig();
            SelectedRepository = null;
        }
    }

    [RelayCommand]
    private Task SelectAppPath() =>
        SelectFolderAsync(path => { Config = Config! with { AppPath = path }; });

    [RelayCommand]
    private Task SelectDownloadPath() =>
        SelectFolderAsync(path => { Config = Config! with { DownloadPath = path }; });

    private async Task SelectFolderAsync(Action<string> callback) {
        var settings = new OpenFolderDialogSettings { Title = _localizer["OpenFolderDialogTitle"] };

        var folder = await _dialogService.ShowOpenFolderDialogAsync(_dialogOwner.Window ?? this, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }

    [RelayCommand]
    private async Task Save(CancellationToken cancellationToken) {
        foreach (var repository in _repositoriesToRemove)
            await _repositoryService.RemoveRepositoryAsync(repository, cancellationToken);

        await _configService.UpdateConfigAsync(Config!, cancellationToken);
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await _configService.GetConfigAsync(cancellationToken);

    private void SetConfig() =>
        Config = Config! with {
            Repositories = Config.Repositories?
                .Where(repository => _repositoriesToRemove.All(removed => removed.Guid != repository.Guid))
                .ToList()
        };
}
