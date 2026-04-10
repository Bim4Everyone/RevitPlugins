using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitSuperfilter.Converters;

internal sealed class LessThanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is double w
           && double.TryParse(parameter?.ToString(), out double limit)) {
            return w < limit;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
