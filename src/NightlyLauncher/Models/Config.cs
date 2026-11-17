using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal sealed record Config(
    string AppPath = "Apps",
    string DownloadPath = "Downloads",
    List<Uri>? Repositories = null);
