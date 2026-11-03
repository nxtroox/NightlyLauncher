using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Localization;

namespace NightlyLauncher.Services;

internal sealed class IoDialogService(IStringLocalizer<IoDialogService> localizer) : IIoDialogService {
    public void OpenFolder(string path) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", path);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", path);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", path);
        else throw new NotImplementedException(localizer["CurrentOperatingSystemNotImplementedExceptionMessage"]);
    }
}
