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

internal partial class SettingsViewModel(
    IConfigService configService,
    IDialogService dialogService,
    IRepositoryService repositoryService,
    IStringLocalizer<SettingsViewModel> localizer)
    : ViewModelBase, IModalDialogViewModel, ICloseable {
    [ObservableProperty] public partial Config? Config { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRepositoryCommand))]
    public partial Repository? SelectedRepository { get; set; }

    private bool CanModifyRepository => SelectedRepository is not null;

    public event EventHandler? RequestClose;

    public bool? DialogResult { get; private set; }

    [RelayCommand]
    private async Task AddRepository(CancellationToken cancellationToken) {
        var vm = dialogService.CreateViewModel<InputViewModel>();

        vm.Title = localizer["UrlInputTitle"];
        vm.Content = localizer["UrlInputContent"];
        vm.TextHint = localizer["UrlInputHint"];

        var result = await dialogService.ShowDialogAsync(this, vm);

        if (result is true) {
            try {
                await repositoryService.AddRepositoryAsync(new Uri(vm.Text!), cancellationToken);
                Config = await configService.GetConfigAsync(cancellationToken);
            } catch (Exception ex) {
                var messageBoxSettings = new MessageBoxSettings {
                    Button = MessageBoxButton.Ok,
                    Content = ex.Message,
                    DefaultValue = true,
                    Icon = MessageBoxImage.Error,
                    Title = localizer["AddRepositoryError"]
                };

                await dialogService.ShowMessageBoxAsync(this, messageBoxSettings);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyRepository))]
    private async Task RemoveRepository(CancellationToken cancellationToken = default) {
        var messageBoxSettings = new MessageBoxSettings {
            Button = MessageBoxButton.YesNo,
            Content = localizer["RemoveRepositoryConfirmationText", SelectedRepository!.ReadableName],
            DefaultValue = false,
            Icon = MessageBoxImage.Warning,
            Title = localizer["RemoveRepositoryConfirmationTitle", SelectedRepository.ReadableName]
        };

        var result = await dialogService.ShowMessageBoxAsync(this, messageBoxSettings);

        if (result is true) {
            await repositoryService.RemoveRepositoryAsync(SelectedRepository, cancellationToken);
            Config = await configService.GetConfigAsync(cancellationToken);
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

        var folder = await dialogService.ShowOpenFolderDialogAsync(this, settings);

        if (folder is not null)
            callback(folder.LocalPath);
    }

    [RelayCommand]
    private async Task Save(CancellationToken cancellationToken) {
        await configService.UpdateConfigAsync(Config!, cancellationToken);
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel() {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    internal async Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Config = await configService.GetConfigAsync(cancellationToken);
}
