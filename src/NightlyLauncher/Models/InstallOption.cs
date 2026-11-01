using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitUri,
    LaunchCommand LaunchCommand,
    string ReadableName,
    Platform Platform,
    PortablePath? PortablePath = null);
