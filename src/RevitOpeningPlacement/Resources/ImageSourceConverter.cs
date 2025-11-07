using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitOpeningPlacement.Resources;

internal class ImageSourceConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is string str
            ? $"../../assets/images/{str}"
            : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
