using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using dosymep.SimpleServices;

using RevitValueModifier.Models;

namespace RevitValueModifier.Views;
/// <summary>
/// Формирует описание параметра для выпадающих списков:
/// "(Общий параметр, Экземпляр, Текст)". Неизвестные части пропускаются
/// </summary>
public class ParamDescriptionConverter : Freezable, IValueConverter {
    public static readonly DependencyProperty LocalizationServiceProperty =
        DependencyProperty.Register(
            "LocalizationService",
            typeof(ILocalizationService),
            typeof(ParamDescriptionConverter),
            new FrameworkPropertyMetadata(null)
            );

    public ILocalizationService LocalizationService {
        get => GetValue(LocalizationServiceProperty) as ILocalizationService;
        set => SetValue(LocalizationServiceProperty, value);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is not RevitParameter revitParameter) {
            return string.Empty;
        }
        string[] parts = [
            GetParamTypeText(revitParameter.ParamTypeName),
            GetParamBindingText(revitParameter.ParamBinding),
            revitParameter.DataTypeName
        ];
        return $"({string.Join(", ", parts.Where(part => !string.IsNullOrEmpty(part)))})";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    protected override Freezable CreateInstanceCore() {
        return new ParamDescriptionConverter();
    }

    private string GetParamTypeText(RevitParamType paramType) {
        if(LocalizationService is null) {
            return paramType.ToString();
        }
        return paramType switch {
            RevitParamType.SystemParameter => LocalizationService.GetLocalizedString("MainWindow.SystemParameter"),
            RevitParamType.SharedParameter => LocalizationService.GetLocalizedString("MainWindow.SharedParameter"),
            RevitParamType.ProjectParameter => LocalizationService.GetLocalizedString("MainWindow.ProjectParameter"),
            RevitParamType.FamilyParameter => LocalizationService.GetLocalizedString("MainWindow.FamilyParameter"),
            _ => paramType.ToString(),
        };
    }

    private string GetParamBindingText(RevitParamBinding paramBinding) {
        if(LocalizationService is null) {
            return paramBinding.ToString();
        }
        return paramBinding switch {
            RevitParamBinding.Instance => LocalizationService.GetLocalizedString("MainWindow.InstanceParameter"),
            RevitParamBinding.Type => LocalizationService.GetLocalizedString("MainWindow.TypeParameter"),
            RevitParamBinding.InstanceAndType => LocalizationService.GetLocalizedString("MainWindow.InstanceAndTypeParameter"),
            _ => paramBinding.ToString(),
        };
    }
}
