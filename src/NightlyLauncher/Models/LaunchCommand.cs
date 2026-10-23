namespace NightlyLauncher.Models;

internal sealed record LaunchCommand(
    string ExecutablePath,
    params string[] Arguments);
