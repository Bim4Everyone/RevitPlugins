using System;
using System.Collections.Generic;
using System.Linq;

using Bim4Everyone.RevitFiltration.Controls;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Предоставляет контексты фильтров поисковых наборов из конфига.
/// <para/>
/// Наборы, сохраненные предыдущими версиями плагина, конвертируются на лету:
/// в новый формат они попадут при первом сохранении конфига в редакторе поисковых наборов.
/// </summary>
internal class FilterContextsProvider {
    private readonly FiltersConfig _config;
    private readonly IFilterContextParser _filterContextParser;
    private readonly LegacyFilterConverter _legacyFilterConverter;

    public FilterContextsProvider(
        FiltersConfig config,
        IFilterContextParser filterContextParser,
        LegacyFilterConverter legacyFilterConverter) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _filterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));
        _legacyFilterConverter = legacyFilterConverter
                                 ?? throw new ArgumentNullException(nameof(legacyFilterConverter));
    }

    /// <summary>
    /// Возвращает названия поисковых наборов и их контексты фильтров.
    /// Наборы, которые не удалось прочитать, пропускаются.
    /// </summary>
    public ICollection<NamedFilterContext> GetFilterContexts() {
        return _config.FilterSettings.Count > 0
            ? GetFilterContexts(_config.FilterSettings).ToArray()
            : GetLegacyFilterContexts().ToArray();
    }

    private IEnumerable<NamedFilterContext> GetFilterContexts(ICollection<FilterSettings> filterSettings) {
        foreach(var settings in filterSettings) {
            if(_filterContextParser.TryParse(settings.FilterContext, out var context)) {
                yield return new NamedFilterContext(settings.Name, context);
            }
        }
    }

    private IEnumerable<NamedFilterContext> GetLegacyFilterContexts() {
        foreach(var legacyFilter in _config.Filters) {
            var provider = _legacyFilterConverter.Convert(legacyFilter);
            if(provider.CanGetFilter(out _)) {
                yield return new NamedFilterContext(legacyFilter.Name, provider.GetFilter());
            }
        }
    }
}
