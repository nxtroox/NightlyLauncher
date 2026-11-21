namespace NightlyLauncher.Core.Models;

public sealed record Repository(
    Guid Guid,
    string DisplayName,
    Uri UpdateUri,
    IReadOnlyList<AppManifest> Apps);
