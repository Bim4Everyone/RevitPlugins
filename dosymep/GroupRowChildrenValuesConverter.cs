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
            return rowData.GetChildrenValues<object>().ToArray().CreateTypedArray(targetType);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var array= values
                .OfType<GroupRowData>()
                .Select(item => Convert(item, targetType, parameter, culture))
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

    internal static class GroupRowExtensions {
        public static T GetGroupRowValue<T>(this GroupRowData rowData) {
            if(rowData?.View.DataControl is GridControl gridControl) {
                return (T) gridControl.GetGroupRowValue(rowData.RowHandle.Value);
            }

            return default;
        }

        public static IEnumerable<T> GetChildrenValues<T>(this GroupRowData rowData) {
            if(rowData?.View.DataControl is GridControl gridControl) {
                return Enumerable.Range(0, gridControl.GetChildRowCount(rowData.RowHandle.Value))
                    .Select(item => gridControl.GetChildRowHandle(rowData.RowHandle.Value, item))
                    .Select(item => gridControl.GetRow(item))
                    .OfType<T>();
            }

            return Enumerable.Empty<T>();
        }

        public static object CreateTypedArray(this Array array, Type arrayType) {
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
