using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace NightlyLauncher.Services;

internal sealed class IoDialogService : IIoDialogService {
    public Task<bool> OpenFolderAsync(string path) {
        var topLevel = GetTopLevel();

        return topLevel is null
            ? throw new InvalidOperationException()
            : topLevel.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
    }

    public Task<bool> OpenUriAsync(Uri uri) {
        var topLevel = GetTopLevel();

        return topLevel is null
            ? throw new InvalidOperationException()
            : topLevel.Launcher.LaunchUriAsync(uri);
    }

    private static TopLevel? GetTopLevel() =>
        Application.Current?.ApplicationLifetime switch {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };
}
