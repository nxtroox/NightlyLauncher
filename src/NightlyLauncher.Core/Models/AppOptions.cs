namespace NightlyLauncher.Core.Models;

public readonly record struct AppOptions(
    LaunchOptions LaunchOptions,
    bool Portable = false,
    bool UpdatePrompt = false);
