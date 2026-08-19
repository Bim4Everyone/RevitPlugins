using System;
using System.Linq;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Конвертирует поисковые наборы, сохраненные версиями плагина до перехода
/// на Bim4Everyone.RevitFiltration, в провайдер фильтра этой библиотеки.
/// <para/>
/// Используется только публичное api библиотеки: старое дерево критериев собирается
/// в <see cref="ILogicalFilter"/>, который вместе с категориями загружается в провайдер.
/// </summary>
internal class LegacyFilterConverter {
    private readonly ILogicalFilterFactory _filterFactory;
    private readonly ILogicalFilterProviderFactory _filterProviderFactory;
    private readonly DataProvider _dataProvider;

    public LegacyFilterConverter(
        ILogicalFilterFactory filterFactory,
        ILogicalFilterProviderFactory filterProviderFactory,
        DataProvider dataProvider) {
        _filterFactory = filterFactory ?? throw new ArgumentNullException(nameof(filterFactory));
        _filterProviderFactory = filterProviderFactory
                                 ?? throw new ArgumentNullException(nameof(filterProviderFactory));
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    /// <summary>
    /// Создает провайдер фильтра по устаревшему поисковому набору.
    /// </summary>
    /// <param name="filter">Устаревший поисковый набор</param>
    /// <returns>Провайдер нового фильтра.</returns>
    /// <exception cref="ArgumentNullException">Исключение, если поисковый набор - пустая ссылка</exception>
    /// <exception cref="InvalidOperationException">Исключение,
    /// если в наборе есть неизвестное условие фильтрации</exception>
    public ILogicalFilterProvider Convert(Filter filter) {
        if(filter is null) {
            throw new ArgumentNullException(nameof(filter));
        }

        var logicalFilter = CreateLogicalFilter(filter.Set);
        var categories = (filter.CategoryIds ?? [])
            .Select(id => id.AsBuiltInCategory())
            .ToArray();
        return _filterProviderFactory.Create(_dataProvider, logicalFilter, categories);
    }

    private ILogicalFilter CreateLogicalFilter(Set set) {
        var logicalFilter = set?.SetEvaluator?.Evaluator == SetEvaluators.Or
            ? _filterFactory.CreateOrFilter()
            : _filterFactory.CreateAndFilter();

        foreach(var criterion in set?.Criteria ?? []) {
            switch(criterion) {
                case Set innerSet:
                    logicalFilter.AddFilter(CreateLogicalFilter(innerSet));
                    break;
                case Rule rule:
                    AddRule(logicalFilter, rule);
                    break;
            }
        }

        return logicalFilter;
    }

    private void AddRule(ILogicalFilter logicalFilter, Rule rule) {
        if(rule.Provider is null
           || rule.Evaluator is null) {
            return;
        }

        new LegacyRuleWriter(rule).Write(logicalFilter);
    }
}
