using System.Runtime.InteropServices;

namespace NightlyLauncher.Models;

internal readonly record struct Platform(
    OsType OsType,
    Architecture Architecture);
