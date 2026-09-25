using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;

using dosymep.SimpleServices;

using RevitClashDetective.Models.Interfaces;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews;

/// <summary>
/// Настройки 3D вида для показа элемента, выбранного в Навигаторе:
/// выделение его основы и подрезка вида по его габаритам.
/// <para>
/// Все настройки применяются за один раз, чтобы их можно было выполнить одним внешним событием Revit.
/// </para>
/// </summary>
internal class NavigatorViewSettings : IView3DSetting {
    /// <summary>
    /// Отступ подрезки вида от габарита элементов в единицах Revit
    /// </summary>
    private const double _sectionBoxOffset = 2;

    private readonly RevitClashDetective.Models.RevitRepository _clashRepository;
    private readonly ILogicalFilterFactory _filterFactory;
    private readonly ILocalizationService _localization;
    private readonly ISelectorAndHighlighter _selectorAndHighlighter;

    public NavigatorViewSettings(
        RevitClashDetective.Models.RevitRepository clashRepository,
        ILogicalFilterFactory filterFactory,
        ILocalizationService localization,
        ISelectorAndHighlighter selectorAndHighlighter) {
        _clashRepository = clashRepository ?? throw new ArgumentNullException(nameof(clashRepository));
        _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _selectorAndHighlighter = selectorAndHighlighter
                                  ?? throw new ArgumentNullException(nameof(selectorAndHighlighter));
    }

    public void Apply(View3D view3D) {
        var settings = GetSettings().ToArray();
        foreach(var setting in settings) {
            setting.Apply(view3D);
        }
    }

    private IEnumerable<IView3DSetting> GetSettings() {
        // выделить можно только основу - стену или перекрытие
        var host = _selectorAndHighlighter.GetElementToHighlight();
        if(host is Wall or Floor) {
            yield return new HostHighlightViewSettings(host, _filterFactory, _localization);
        }

        yield return new SectionBoxViewSettings(
            _clashRepository,
            _selectorAndHighlighter.GetElementsToSelect(),
            _sectionBoxOffset);
    }
}
