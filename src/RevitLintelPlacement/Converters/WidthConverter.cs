using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitLintelPlacement.Converters;

internal class WidthConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is double width
            ? width - (int.TryParse(parameter.ToString(), out int result) ? result : 0)
            : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is int width
            ? width + (int.TryParse(parameter.ToString(), out int result) ? result : 0)
            : value;
    }
}
