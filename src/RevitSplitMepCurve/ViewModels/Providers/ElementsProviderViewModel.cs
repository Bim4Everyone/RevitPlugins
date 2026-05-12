using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Settings;
using RevitSplitMepCurve.Services.Providers;

namespace RevitSplitMepCurve.ViewModels.Providers;

internal abstract class ElementsProviderViewModel : BaseViewModel {
    protected readonly ILocalizationService _localization;

    protected ElementsProviderViewModel(ILocalizationService localization, IElementsProvider provider) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public abstract string Name { get; }

    public abstract MepClass MepClass { get; }

    public IElementsProvider Provider { get; }

    /// <summary>Текст ошибки валидации. Пусто если всё ок.</summary>
    public abstract string GetErrorText();

    /// <summary>Настройки разделения для выбранных уровней и текущих соединителей.</summary>
    public abstract ISplitSettings GetSplitSettings(ICollection<Level> levels);
}
