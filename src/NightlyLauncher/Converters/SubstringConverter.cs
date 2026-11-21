using System.Globalization;
using Avalonia.Data.Converters;

namespace NightlyLauncher.Converters;

public sealed class SubstringConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is string str && int.TryParse(parameter?.ToString(), out var length))
            return str.Length > length ? str[..length] : str;

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
