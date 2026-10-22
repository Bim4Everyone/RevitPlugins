using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitPackageDocumentation.Views.Converters;

/// <summary>
/// Преобразует объект в коллекцию из одного этого объекта (null - в пустую коллекцию).
/// Используется, чтобы при смене объекта ItemsControl пересоздавал шаблон, а не перепривязывал существующие контролы
/// </summary>
internal class SingleItemCollectionConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is null ? Array.Empty<object>() : new[] { value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
