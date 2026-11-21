namespace NightlyLauncher.Models;

internal record Repository(
    Guid Guid,
    string DisplayName,
    Uri UpdateUri,
    IReadOnlyList<AppManifest> Apps);
