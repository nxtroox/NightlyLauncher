using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record AppManifest(
    Guid Guid,
    string DisplayName,
    IReadOnlyList<InstallOption> InstallOptions,
    Uri? IconUri = null);
