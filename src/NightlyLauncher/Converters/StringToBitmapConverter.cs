using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace NightlyLauncher.Converters;

public sealed class StringToBitmapConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not string path || !File.Exists(path))
            return null;

        try {
            return new Bitmap(path);
        } catch {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
