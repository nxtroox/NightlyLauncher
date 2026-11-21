namespace NightlyLauncher.Core.Models;

public readonly record struct RepositoryEntry(
    Uri Uri,
    Guid? Guid = null);
