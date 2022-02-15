using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RevitLintelPlacement.Converters {
    internal class WidthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is double width ? width - (Int32.TryParse(parameter.ToString(), out int result) ? result : 0) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is int width ? width + (Int32.TryParse(parameter.ToString(), out int result) ? result : 0) : value;
        }
    }
}
