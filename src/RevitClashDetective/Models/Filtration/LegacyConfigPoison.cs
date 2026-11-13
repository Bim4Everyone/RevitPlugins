using System;
using System.Collections.Generic;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Создает содержимое устаревшего свойства <see cref="FiltersConfig.Filters"/> для нового конфига.
/// <para/>
/// Записывается ровно один поисковый набор с одним правилом, параметр которого -
/// <see cref="You_must_update_plugin"/>. Старые версии плагина падают на нем при десериализации
/// и не могут перезаписать новый конфиг старым форматом.
/// </summary>
internal static class LegacyConfigPoison {
    public static List<Filter> Create(RevitRepository revitRepository) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }

        return [
            new Filter(revitRepository) {
                Name = nameof(You_must_update_plugin),
                CategoryIds = [],
                Set = new Set() {
                    Criteria = [
                        new Rule() {
                            Evaluator = new RuleEvaluator() {
                                Evaluator = RuleEvaluators.FilterStringEquals,
                                Message = nameof(You_must_update_plugin)
                            },
                            Value = new StringParamValue(nameof(You_must_update_plugin)),
                            Provider = new ParameterValueProvider(
                                revitRepository,
                                new You_must_update_plugin(),
                                nameof(You_must_update_plugin))
                        }
                    ]
                }
            }
        ];
    }
}
