namespace NightlyLauncher.Core.Models;

public sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    string DisplayName,
    Platform Platform,
    AppOptions AppOptions,
    PortableOptions? PortablePath = null);
