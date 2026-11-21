namespace NightlyLauncher.Core.Models;

public readonly record struct LaunchOptions(
    string ExecutablePath,
    IReadOnlyList<string>? Arguments = null);
