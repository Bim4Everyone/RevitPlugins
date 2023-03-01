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
    internal class GroupRowValueConverter : IValueConverter, IMultiValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GroupRowData rowData = value as GroupRowData;
            if(rowData?.View.DataControl is GridControl gridControl) {
                return gridControl.GetGroupRowValue(rowData.RowHandle.Value);
            }

            return default;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var array = values
                .OfType<GroupRowData>()
                .Select(item => Convert(item, targetType, parameter, culture))
                .Where(item => item != default)
                .ToArray();

            return array.CreateTypedArray(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
