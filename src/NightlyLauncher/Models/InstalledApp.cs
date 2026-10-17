using System;

namespace NightlyLauncher.Models;

internal sealed record InstalledApp(
    AppManifest Manifest,
    string? LocalVersion = null,
    string? LatestVersion = null,
    DateTime? InstalledOn = null);
