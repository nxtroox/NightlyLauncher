using Avalonia.Data.Converters;

namespace NightlyLauncher.Converters;

public static class CollectionConverters {
    public static FuncValueConverter<int, bool> HasMultipleItems { get; } =
        new(count => count > 1);
}
