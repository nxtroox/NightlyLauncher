using System.Runtime.InteropServices;

namespace NightlyLauncher.Core.Models;

public readonly record struct Platform(
    OsType OsType,
    Architecture Architecture);
