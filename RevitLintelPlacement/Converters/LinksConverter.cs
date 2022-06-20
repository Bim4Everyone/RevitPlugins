using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Converters {
    internal class LinksConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is List<object> valueList) {
                return valueList.Select(item => item as LinkViewModel).ToList();
            }
            return new List<LinkViewModel>();
        }
    }
}
