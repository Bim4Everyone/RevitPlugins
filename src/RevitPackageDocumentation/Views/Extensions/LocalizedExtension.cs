using System;
using System.Windows.Markup;

using dosymep.SimpleServices;

namespace RevitPackageDocumentation.Views.Extensions;
/// <summary>
/// Требуется для выполнения локализации ресурсов в библиотеке ресурсов DataTemplate
/// </summary>
public class LocalizedExtension : MarkupExtension {
    private static ILocalizationService _localizationService;
    private readonly string _key;

    public LocalizedExtension(string key) {
        _key = key;
    }

    public static void Init(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        if(_localizationService == null)
            return _key;
        return _localizationService.GetLocalizedString(_key);
    }
}
