namespace NightlyLauncher.Models;

internal readonly record struct ProgressReport(
    int Percentage,
    string StatusText);
