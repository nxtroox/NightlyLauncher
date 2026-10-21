using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record AppManifest(
    Guid Guid,
    string ReadableName,
    IEnumerable<InstallOption> InstallOptions,
    Guid? SelectedInstallOptionGuid = null,
    bool UpdatePrompt = false,
    bool Portable = false);
