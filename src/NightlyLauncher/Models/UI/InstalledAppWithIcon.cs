namespace NightlyLauncher.Models.UI;

internal sealed record InstalledAppWithIcon(
    InstalledApp App,
    string? IconPath = null);
