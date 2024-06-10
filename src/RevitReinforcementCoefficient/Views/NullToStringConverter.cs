using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitReinforcementCoefficient.Views {
    internal class NullToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is string ? (string.IsNullOrEmpty((string) value) ? "<Нет данных>" : value) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
