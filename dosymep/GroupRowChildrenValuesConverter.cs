using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using DevExpress.Xpf.Grid;
using DevExpress.XtraSpreadsheet.Utils;

namespace dosymep.WPF.Converters {
    internal class GroupRowChildrenValuesConverter : IValueConverter, IMultiValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetChildrenValues(value as GroupRowData).CreateTypedArray(targetType);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var array = values
                .OfType<GroupRowData>()
                .Select(item => GetChildrenValues(item))
                .SelectMany(item => item)
                .ToArray();

            return array.CreateTypedArray(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private object[] GetChildrenValues(GroupRowData value) {
            return value.GetChildrenValues<object>().ToArray();
        }
    }
}
