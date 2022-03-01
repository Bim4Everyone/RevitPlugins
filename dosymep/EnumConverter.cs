using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using dosymep.WPF.Extensions;

namespace dosymep.WPF.Converters {
    internal class EnumConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || parameter == null) {
                return string.Empty;
            }
            foreach(var one in Enum.GetValues(parameter as Type)) {
                if(value.Equals(one))
                    return ((IConvertible) one).GetDescription();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null)
                return null;
            foreach(var one in Enum.GetValues(parameter as Type)) {
                if(value.ToString() == ((IConvertible) one).GetDescription())
                    return one;
            }
            return null;
        }
    }
}