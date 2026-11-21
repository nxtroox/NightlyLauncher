namespace NightlyLauncher.Core.Models;

public sealed record AppManifest(
    Guid Guid,
    Uri LatestCommitApiUri,
    string DisplayName,
    IReadOnlyList<InstallOption> InstallOptions,
    Uri? IconUri = null);
