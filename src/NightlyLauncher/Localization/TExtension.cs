using Avalonia.Markup.Xaml;

namespace NightlyLauncher.Localization;

public sealed class TExtension(string key) : MarkupExtension {
    private Type? Source { get; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        var type = Source
                   ?? (serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject
                   .GetType()
                   ?? throw new InvalidOperationException("No resource type found.");

        return LocalizationManager.Instance.Get(type, key);
    }
}
