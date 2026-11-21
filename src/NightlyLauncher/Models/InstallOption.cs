namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitApiUri,
    string DisplayName,
    Platform Platform,
    AppOptions AppOptions,
    PortableOptions? PortablePath = null);
