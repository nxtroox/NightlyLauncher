using Microsoft.Extensions.Localization;

namespace NightlyLauncher.Localization;

public sealed class LocalizationManager(IStringLocalizerFactory factory) {
    public static LocalizationManager Instance { get; set; } = null!;
    public string Get(Type type, string key) => factory.Create(type)[key].Value;
}
