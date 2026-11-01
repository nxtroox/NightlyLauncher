using System;

namespace NightlyLauncher.Models;

internal sealed record InstalledApp(
    AppManifest Manifest,
    Guid SelectedInstallOptionGuid,
    string? LocalVersion = null,
    string? LatestVersion = null,
    DateTime? UpdatedOn = null);
