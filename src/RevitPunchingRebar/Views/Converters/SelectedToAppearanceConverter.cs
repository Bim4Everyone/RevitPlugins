using System;
using System.Globalization;
using System.Windows.Data;

using Wpf.Ui.Controls;

namespace RevitPunchingRebar.Views.Converters;

/// <summary>
/// Выбирает оформление кнопки «Выбрать»: акцентное, пока элементы не выбраны, обычное — после выбора.
/// </summary>
public class SelectedToAppearanceConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is true ? ControlAppearance.Secondary : ControlAppearance.Info;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
