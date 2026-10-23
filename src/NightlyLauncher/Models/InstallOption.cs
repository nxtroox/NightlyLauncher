using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitUri,
    LaunchCommand LaunchCommand,
    PortablePath? PortablePath = null,
    string? ReadableName = null);
