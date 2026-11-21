namespace NightlyLauncher.Models;

internal readonly record struct RepositoryEntry(
    Uri Uri,
    Guid? Guid = null);
