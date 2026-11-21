namespace NightlyLauncher.Core.Models;

public sealed record AppManifest(
    Guid Guid,
    string DisplayName,
    IReadOnlyList<InstallOption> InstallOptions,
    Uri? IconUri = null);
