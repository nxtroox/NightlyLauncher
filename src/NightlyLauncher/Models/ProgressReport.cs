namespace NightlyLauncher.Models;

internal sealed record ProgressReport(
    int Percentage,
    string StatusText);
