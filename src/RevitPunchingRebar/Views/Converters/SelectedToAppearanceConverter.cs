using System;
using System.Globalization;
using System.Windows.Data;

using Wpf.Ui.Controls;

namespace RevitPunchingRebar.Views.Converters;

/// <summary>
/// Выбирает оформление кнопки «Выбрать»: акцентное, пока элементы не выбраны, обычное — после выбора.
/// </summary>
public class SelectedToAppearanceConverter : IValueConverter {
    /// <summary>
    /// Оформление, когда элементы выбраны.
    /// </summary>
    public ControlAppearance SelectedAppearance { get; set; } = ControlAppearance.Secondary;

    /// <summary>
    /// Оформление, когда элементы не выбраны.
    /// </summary>
    public ControlAppearance NotSelectedAppearance { get; set; } = ControlAppearance.Info;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is true ? SelectedAppearance : NotSelectedAppearance;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
