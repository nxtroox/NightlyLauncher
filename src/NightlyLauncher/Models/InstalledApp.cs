using System;

namespace NightlyLauncher.Models;

internal sealed record InstalledApp(
    AppManifest Manifest,
    string LocalVersion,
    Guid? SelectedInstallOptionGuid = null,
    string? LatestVersion = null,
    DateTime? UpdatedOn = null);
