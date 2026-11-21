namespace NightlyLauncher.Models;

internal sealed record LaunchOptions(
    string ExecutablePath,
    IReadOnlyList<string>? Arguments = null);
