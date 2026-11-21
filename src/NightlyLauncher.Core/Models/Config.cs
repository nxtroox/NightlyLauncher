namespace NightlyLauncher.Core.Models;

public sealed record Config(
    string AppPath = "Apps",
    string DownloadPath = "Downloads",
    List<RepositoryEntry>? Repositories = null);
