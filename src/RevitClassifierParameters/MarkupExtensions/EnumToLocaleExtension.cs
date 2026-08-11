using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using dosymep.SimpleServices;
using dosymep.WpfCore;

namespace RevitClassifierParameters.MarkupExtensions;

internal sealed class EnumToLocaleExtension : MarkupExtension, IValueConverter {
    private IHasLocalization _hasLocalization;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(_hasLocalization is null) {
            return value;
        }

        string enumName = $"{value?.GetType().Name}.{value}";
        return _hasLocalization.LocalizationService.GetLocalizedString(enumName);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        _hasLocalization = serviceProvider.GetRootObject<IHasLocalization>();
        if(_hasLocalization is not null) {
            return this;
        }

        var frameworkElement = serviceProvider.GetRootObject<FrameworkElement>();
        if(frameworkElement is not null) {
            frameworkElement.Loaded += FrameworkElementOnLoaded;
        }

        return this;
    }

    private void FrameworkElementOnLoaded(object sender, RoutedEventArgs e) {
        if(sender is IHasLocalization hasLocalization) {
            _hasLocalization = hasLocalization;
        }

        if(sender is FrameworkElement frameworkElement) {
            frameworkElement.Loaded -= FrameworkElementOnLoaded;
        }
    }
}
