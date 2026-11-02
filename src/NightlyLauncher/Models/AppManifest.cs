using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record AppManifest(
    Guid Guid,
    string DisplayName,
    IEnumerable<InstallOption> InstallOptions,
    bool UpdatePrompt = false,
    bool Portable = false);
