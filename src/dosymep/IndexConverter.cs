using System;
using System.Globalization;
using System.Windows.Data;

namespace dosymep.WPF.Converters {
    internal class IndexConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is int index) {
                return index + 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is int index) {
                return index - 1;
            }
            return value;
        }
    }
}