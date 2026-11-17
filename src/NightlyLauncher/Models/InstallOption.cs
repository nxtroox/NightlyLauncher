using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitApiUri,
    LaunchOptions LaunchOptions,
    string DisplayName,
    Platform Platform,
    PortableOptions? PortablePath = null);
