using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterValueProvider : IFilterableValueProvider {
        private readonly RevitRepository _revitRepository;

        public RevitParam RevitParam { get; set; }

        public string Name => RevitParam.Name;

        public ParameterValueProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public IEnumerable<RuleEvaluator> GetRuleEvaluators() {
            return RuleEvaluatorUtils.GetRuleEvaluators(RevitParam.StorageType);
        }

        public IEnumerable<object> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator) {
            if(ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue || ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue) {
                var categoryFilter = new ElementMulticategoryFilter(categories.Select(item => item.Id).ToList());
                return _revitRepository.GetCollector()
                    .WherePasses(categoryFilter)
                    .Select(item => item.GetParamValueOrDefault(RevitParam))
                    .Distinct();
            }
            return null;
        }
    }
}
