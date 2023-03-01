using System;
using System.Collections.Generic;
using System.Linq;

using DevExpress.Xpf.Grid;

namespace dosymep.WPF {
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