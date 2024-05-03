using System;
using System.Globalization;
using System.Windows.Data;

namespace dosymep.WPF.Converters {
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(targetType != typeof(bool) && targetType != typeof(bool?)) {
                throw new InvalidOperationException("The target must be a boolean");
            }

            bool? b = (bool?) value;
            return b.HasValue && !b.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(value as bool?);
        }
    }
}