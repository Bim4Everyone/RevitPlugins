using System;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews;
/// <summary>
/// Настройки 3D вида для графического выделения основы (стены или перекрытия).
/// <para>
/// Выделение происходит при помощи фильтров вида, в которые попадают все конструкции (стены и перекрытия),
/// за исключением заданной основы, с последующим изменением их графики.
/// </para>
/// </summary>
internal class HostHighlightViewSettings : IView3DSetting {
    private readonly Element _host;
    private readonly ILogicalFilterFactory _filterFactory;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Конструктор настроек 3D вида для графического выделения основы
    /// </summary>
    /// <param name="host">Основа, которую нужно выделить. Это должна быть стена или перекрытие</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <param name="localization">Сервис локализации</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если основа не стена и не перекрытие</exception>
    public HostHighlightViewSettings(
        Element host,
        ILogicalFilterFactory filterFactory,
        ILocalizationService localization) {

        if(host is null) {
            throw new ArgumentNullException(nameof(host));
        }
        _host = host is Wall or Floor
            ? host
            : throw new ArgumentException(nameof(host));
        _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public void Apply(View3D view3D) {
        var doc = view3D.Document;
        var filters = ParameterFilterInitializer.GetHighlightFilters(doc, _host, _filterFactory);
        var graphicsSettings = GraphicSettingsInitializer.GetNotInterestingConstructionsGraphicSettings();
        using var t = doc.StartTransaction(_localization.GetLocalizedString("Transaction.HighlightHost"));

        foreach(var filter in filters) {
            if(view3D.GetFilters().Contains(filter.Id)) {
                view3D.RemoveFilter(filter.Id);
            }
            view3D.AddFilter(filter.Id);
            view3D.SetFilterOverrides(filter.Id, graphicsSettings);
            view3D.SetFilterVisibility(filter.Id, true);
        }

        t.Commit();
    }
}
