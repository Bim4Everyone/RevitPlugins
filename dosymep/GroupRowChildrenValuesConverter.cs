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
            GroupRowData rowData = value as GroupRowData;
            if(rowData?.View.DataControl is GridControl gridControl) {
                Array array = Enumerable.Range(0, gridControl.GetChildRowCount(rowData.RowHandle.Value))
                    .Select(item => gridControl.GetChildRowHandle(rowData.RowHandle.Value, item))
                    .Select(item => gridControl.GetRow(item))
                    .ToArray();

                return CreateTypedArray(array, targetType);
            }

            return CreateTypedArray(Array.Empty<object>(), targetType);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var groupRow = values.OfType<GroupRowData>().FirstOrDefault();
            return Convert(groupRow, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public object CreateTypedArray(Array array, Type arrayType) {
            if(arrayType == null
               || arrayType == typeof(object)) {
                return array;
            }

            var newArray = Array.CreateInstance(arrayType, array.Length);
            array.CopyTo(newArray, default);
            return newArray;
        }
    }
}
