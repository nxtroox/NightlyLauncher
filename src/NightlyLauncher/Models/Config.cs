namespace NightlyLauncher.Models;

internal sealed record Config(
    string AppPath = "Apps",
    string CachePath = "Cache",
    List<RepositoryEntry>? Repositories = null,
    bool CloseAfterLaunch = true);
