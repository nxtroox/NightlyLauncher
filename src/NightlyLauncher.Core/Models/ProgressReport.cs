namespace NightlyLauncher.Core.Models;

public readonly record struct ProgressReport(
    int Percentage,
    string StatusText);
