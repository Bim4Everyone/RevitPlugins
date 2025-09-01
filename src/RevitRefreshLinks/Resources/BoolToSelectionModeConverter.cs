using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace RevitRefreshLinks.Resources;
[ValueConversion(typeof(bool), typeof(SelectionMode))]
internal class BoolToSelectionModeConverter : IValueConverter {
    public SelectionMode TrueValue { get; set; }

    public SelectionMode FalseValue { get; set; }

    public BoolToSelectionModeConverter() {
        TrueValue = SelectionMode.Extended;
        FalseValue = SelectionMode.Single;
    }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is not bool ? null : (bool) value ? TrueValue : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if(Equals(value, TrueValue)) {
            return true;
        }

        return Equals(value, FalseValue) ? false : null;
    }
}
