namespace NightlyLauncher.Core.Models;

public sealed record InstalledApp(
    AppManifest Manifest,
    Guid InstallOptionGuid,
    AppOptions Options,
    string? LocalVersion = null,
    DateTime? LastUpdatedOn = null);
