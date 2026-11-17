namespace NightlyLauncher.Models;

internal sealed record AppOptions(
    LaunchOptions LaunchOptions,
    bool Portable = false,
    bool UpdatePrompt = false);
