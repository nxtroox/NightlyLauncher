using System;

namespace NightlyLauncher.Models;

internal sealed record InstallOption(
    Guid Guid,
    Uri DownloadUri,
    Uri LatestCommitUri,
    PortablePath? PortablePath = null,
    string? ReadableName = null);
