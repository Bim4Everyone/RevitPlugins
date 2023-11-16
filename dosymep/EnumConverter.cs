using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using dosymep.WPF.Extensions;

namespace dosymep.WPF.Converters {
    internal class EnumConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return null;
            }

            IConvertible convertible = Enum.GetValues((Type) parameter)
                .OfType<IConvertible>()
                .FirstOrDefault(item => item.Equals(value));

            return convertible?.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return null;
            }

            string stringValue = value.ToString();
            return Enum.GetValues((Type) parameter)
                .OfType<IConvertible>()
                .FirstOrDefault(item => item.GetDescription().Equals(stringValue));
        }
    }
}