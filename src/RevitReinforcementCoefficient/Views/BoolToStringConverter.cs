using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitReinforcementCoefficient.Views {
    internal class BoolToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool) value == true ? "Да" : "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (string) value == "Да" ? true : false;
        }
    }
}
