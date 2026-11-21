using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace NightlyLauncher.Localization;

public class TExtension(string key) : MarkupExtension {
    private string Key { get; } = key;

    public Type? Source { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        var type = Source
                   ?? (serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject
                   .GetType()
                   ?? throw new InvalidOperationException("No resource type found.");

        return new Binding($"[{Key}]") { Source = LocalizationManager.Instance.For(type), Mode = BindingMode.OneWay };
    }
}
