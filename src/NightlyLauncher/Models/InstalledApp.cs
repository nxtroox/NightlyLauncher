using System;

namespace NightlyLauncher.Models;

internal sealed record InstalledApp(
    AppManifest Manifest,
    string LocalVersion,
    Guid SelectedInstallOptionGuid,
    bool IsInstalled = false,
    string? LatestVersion = null,
    DateTime? UpdatedOn = null);
