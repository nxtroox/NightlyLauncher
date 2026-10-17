using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record AppManifest(
    IEnumerable<InstallOperation> InstallOptions,
    string ReadableName,
    bool UpdatePrompt = false,
    bool Portable = false);
