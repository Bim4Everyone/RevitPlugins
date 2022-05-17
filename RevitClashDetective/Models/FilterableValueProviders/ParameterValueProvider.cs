using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit.Comparators;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterValueProvider : IFilterableValueProvider {
        private readonly RevitRepository _revitRepository;

        public RevitParam RevitParam { get; set; }

        public string Name => RevitParam.Name;

        public IFilterGenerator FilterGenerator { get; }

        public ParameterValueProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public IEnumerable<RuleEvaluator> GetRuleEvaluators() {
            return RuleEvaluatorUtils.GetRuleEvaluators(RevitParam.StorageType);
        }

        public IEnumerable<ParamValue> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator) {
            if(ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue && ruleEvaluator.Evaluator != RuleEvaluators.FilterHasValue) {
                var categoryFilter = new ElementMulticategoryFilter(categories.Select(item => item.Id).ToList());
                var collecotor = _revitRepository.GetClashCollector();
                if(collecotor != null) {
                    return collecotor
                        .WherePasses(categoryFilter)
                        .WhereElementIsNotElementType()
                        .Select(GetElementParam)
                        .Where(item => item != null && item.Value != null && item.Value as ElementId != ElementId.InvalidElementId)
                        .Distinct()
                        .OrderBy(item => item, new ParamValueComparer());

                }
            }
            return Enumerable.Empty<ParamValue>();
        }

        private ParamValue GetElementParam(Element item) {
            if(item.IsExistsParam(RevitParam)) {
                return new ParamValue(item.GetParamValueOrDefault(RevitParam), item.GetParamValueStringOrDefault(RevitParam));
            } else {
                var typeId = item.GetTypeId();
                if(typeId != null) {
                    var type = _revitRepository.GetElement(typeId);
                    return new ParamValue(type.GetParamValueOrDefault(RevitParam), type.GetParamValueStringOrDefault(RevitParam));
                }
            }
            return null;
        }

        public FilterRule GetRule(IRevitRuleCreator creator, object value) {
            ElementId id = null;
            if(RevitParam is SystemParam systemParam) {
#if D2020 || R2020
                id = new ElementId(systemParam.SystemParamId);
#endif
            } else {
                id = RevitParam.GetRevitParamElement(_revitRepository.Doc).Id;
            }
            return creator.Create(RevitParam.StorageType, id, value);
        }
    }
}
