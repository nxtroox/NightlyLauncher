using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record LaunchCommand(
    string ExecutablePath,
    List<string>? Arguments = null);
