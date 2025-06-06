using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using dosymep.SimpleServices;

using RevitValueModifier.Models;

namespace RevitValueModifier.Views;
public class ParamTypeConverter : Freezable, IValueConverter {
    protected override Freezable CreateInstanceCore() {
        return new ParamTypeConverter();
    }

    public static readonly DependencyProperty LocalizationServiceProperty =
        DependencyProperty.Register(
            "LocalizationService",
            typeof(ILocalizationService),
            typeof(ParamTypeConverter),
            new FrameworkPropertyMetadata(null)
            );

    public ILocalizationService LocalizationService {
        get => GetValue(LocalizationServiceProperty) as ILocalizationService; 
        set => SetValue(LocalizationServiceProperty, value);
    }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var paramType = (RevitParamType) value;
        if(LocalizationService is null) {
            return paramType.ToString();
        }
        return paramType switch {
            RevitParamType.SystemParameter => LocalizationService.GetLocalizedString("MainWindow.SystemParameter"),
            RevitParamType.SharedParameter => LocalizationService.GetLocalizedString("MainWindow.SharedParameter"),
            RevitParamType.ProjectParameter => LocalizationService.GetLocalizedString("MainWindow.ProjectParameter"),
            _ => paramType.ToString(),
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
