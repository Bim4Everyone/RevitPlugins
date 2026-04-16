using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using RevitSuperfilter.ViewModels;

namespace RevitSuperfilter.Converters;

internal sealed class DisplayValueConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        // values[0] - это наш ViewModel
        // values[1] - это сам UI элемент (TextBlock)

        if(values[0] is null
           || values[1] is not FrameworkElement targetElement) {
            return string.Empty;
        }

        dynamic vm = values[0];
        if(vm.DisplayValue is null) {
            return TryFindResource(values[0], targetElement, "DefaultName");
        } else if(string.IsNullOrEmpty(vm.DisplayValue)) {
            return TryFindResource(values[0], targetElement, "EmptyName");
        }

        return vm.DisplayValue;
    }

    private static object TryFindResource(object value, FrameworkElement targetElement, string resourceName) {
        string resourceKey = value switch {
            CategoryViewModel => $"Category.{resourceName}",
            DefinitionViewModel => $"Definition.{resourceName}",
            ParamValueViewModel => $"ParamValue.{resourceName}",
            _ => throw new ArgumentOutOfRangeException()
        };

        return targetElement.TryFindResource(resourceKey) as string ?? resourceKey;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
