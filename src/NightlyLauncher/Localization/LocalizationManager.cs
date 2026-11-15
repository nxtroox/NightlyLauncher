using System;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace NightlyLauncher.Localization;

public sealed class LocalizationManager(IStringLocalizerFactory factory) {
    private readonly ConcurrentDictionary<Type, LocalizedSource> _sources = new();

    public static LocalizationManager Instance { get; set; } = null!;

    public LocalizedSource For(Type type)
        => _sources.GetOrAdd(type, t => new LocalizedSource(factory.Create(t)));

    public void SetCulture(CultureInfo culture) {
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentUICulture = culture;

        foreach (var source in _sources.Values)
            source.Refresh();
    }
}
