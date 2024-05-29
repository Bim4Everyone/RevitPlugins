using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using Autodesk.Revit.DB;

namespace RevitGenLookupTables.Converters {
    internal class StorageTypeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is StorageType s) {
                switch(s) {
                    case StorageType.None:
                    return "Нет";
                    case StorageType.Integer:
                    return "Целое число";
                    case StorageType.Double:
                    return "Вещественное число";
                    case StorageType.ElementId:
                    return "Идентификатор элемента";
                    case StorageType.String:
                    return "Строка";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
