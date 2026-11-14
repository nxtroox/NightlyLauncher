using System;
using FluentIcons.Common;

namespace NightlyLauncher.Models;

internal sealed record NavigationItem(
    string Label,
    Symbol Symbol,
    Type TargetViewModelType);
