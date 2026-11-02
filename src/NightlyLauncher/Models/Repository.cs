using System;
using System.Collections.Generic;

namespace NightlyLauncher.Models;

internal record Repository(
    Guid Guid,
    List<AppManifest> Apps,
    string DisplayName,
    Uri? UpdateUri = null);
