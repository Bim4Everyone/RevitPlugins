using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using DevExpress.Xpf.Grid;

namespace RevitCheckingLevels.Converters {
    internal class GroupRowConverter : IValueConverter, IMultiValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            GroupRowData rowData = (GroupRowData) value;
            GridControl gridControl = (GridControl) rowData.View.DataControl;

            return Enumerable.Range(0, gridControl.GetChildRowCount(rowData.RowHandle.Value))
                .Select(item => gridControl.GetChildRowHandle(rowData.RowHandle.Value, item))
                .Select(item => gridControl.GetRow(item))
                .ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return Convert(values[0], targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
