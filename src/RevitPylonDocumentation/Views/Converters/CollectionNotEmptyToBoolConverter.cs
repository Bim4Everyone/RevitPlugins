using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace RevitPylonDocumentation.Views.Converters;

public class CollectionNotEmptyToBoolConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value == null)
            return false;

        if(value is IEnumerable enumerable) {
            // Если MoveNext возвращает true, то первый элемент коллекции удалось вернуть, значит она не пуста
            var enumerator = enumerable.GetEnumerator();
            return enumerator.MoveNext();
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
