using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitUri,
    LaunchCommand LaunchCommand,
    string DisplayName,
    Platform Platform,
    PortablePath? PortablePath = null);
