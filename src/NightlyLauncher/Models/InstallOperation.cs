using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOperation(
    Uri DownloadUri,
    Uri LatestCommitUri,
    string InstallPath,
    PortablePath? PortablePath = null,
    string? ReadableName = null);
