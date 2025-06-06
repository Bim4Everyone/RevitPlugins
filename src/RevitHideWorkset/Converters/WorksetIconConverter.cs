using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitHideWorkset.Converters;
public class WorksetIconConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool isOpen ? isOpen ? "Eye16" : "EyeOff16" : "Eye16";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
