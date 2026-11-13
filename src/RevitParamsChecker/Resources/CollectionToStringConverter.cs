using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RevitParamsChecker.Resources;

internal class CollectionToStringConverter : IValueConverter {
    public string Separator { get; set; } = "; ";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is IEnumerable<object> items
            ? string.Join(Separator, items.Select(i => i.ToString()))
            : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
