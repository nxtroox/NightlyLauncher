using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using NightlyLauncher.Services;

namespace NightlyLauncher.ViewModels;

internal sealed partial class AboutDialogViewModel : ViewModelBase {
    private readonly IIoDialogService _ioDialogService;

    public AboutDialogViewModel(IStringLocalizer<AboutDialogViewModel> localizer, IIoDialogService dialogService) {
        _ioDialogService = dialogService;

        Title = localizer["AboutDialogViewTitle"];
        AuthorUri = new Uri("https://github.com/nxtroox");
    }

    public ObservableCollection<string> Libraries { get; private set; } = [
        "Avalonia",
        "Avalonia.Desktop",
        "CommunityToolkit.Mvvm",
        "FluentAvaloniaUI",
        "FluentIcons.Avalonia.Fluent",
        "HanumanInstitute.MvvmDialogs",
        "HanumanInstitute.MvvmDialogs.Avalonia",
        "HanumanInstitute.MvvmDialogs.Avalonia.Fluent",
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Extensions.Http",
        "Microsoft.Extensions.Localization"
    ];

    public Uri AuthorUri { get; private set; }

    [RelayCommand]
    private Task<bool> OpenLibrary(string nuGetPackageName) =>
        _ioDialogService.OpenUriAsync(new Uri("https://www.nuget.org/packages/" + nuGetPackageName));
}
