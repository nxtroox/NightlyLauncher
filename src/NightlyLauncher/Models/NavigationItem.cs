using System;
using FluentIcons.Common;

namespace NightlyLauncher.Models;

internal sealed record NavigationItem(
    string Title,
    Symbol Symbol,
    Type TargetViewModelType);
