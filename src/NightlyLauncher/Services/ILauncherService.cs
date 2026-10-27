using System;
using NightlyLauncher.Models;

namespace NightlyLauncher.Services;

internal interface ILauncherService {
    void LaunchApp(InstalledApp app, IProgress<ProgressReport> progress);
}
