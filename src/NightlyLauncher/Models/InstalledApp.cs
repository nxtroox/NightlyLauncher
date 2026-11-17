using System;

namespace NightlyLauncher.Models;

internal sealed record InstalledApp(
    AppManifest Manifest,
    Guid InstallOptionGuid,
    AppOptions Options,
    string? LocalVersion = null,
    DateTime? LastUpdatedOn = null);
