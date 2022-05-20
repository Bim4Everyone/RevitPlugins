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

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterValueProvider : IFilterableValueProvider, IEquatable<ParameterValueProvider> {

        public RevitParam RevitParam { get; set; }

        public string Name => RevitParam.Name;

        [JsonIgnore]
        public IFilterGenerator FilterGenerator { get; }
        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }

        public ParameterValueProvider(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
        }

        public IEnumerable<RuleEvaluator> GetRuleEvaluators() {
            return RuleEvaluatorUtils.GetRuleEvaluators(RevitParam.StorageType);
        }

        public IEnumerable<ParamValueViewModel> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator) {
            if(ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue && ruleEvaluator.Evaluator != RuleEvaluators.FilterHasValue) {
                var categoryFilter = new ElementMulticategoryFilter(categories.Select(item => item.Id).ToList());
                var collector = RevitRepository.GetClashCollector();
                if(collector != null) {
                    return collector
                        .WherePasses(categoryFilter)
                        .WhereElementIsNotElementType()
                        .Select(GetElementParam)
                        .Where(item => item != null && item.Value != null && item.Value as ElementId != ElementId.InvalidElementId)
                        .Distinct()
                        .OrderBy(item => item.ParamValue);

                }
            }
            return Enumerable.Empty<ParamValueViewModel>();
        }

        private ParamValueViewModel GetElementParam(Element item) {

            if(item.IsExistsParam(RevitParam)) {
                return new ParamValueViewModel(RevitParam, item);
            } else {
                var typeId = item.GetTypeId();
                if(typeId != null) {
                    var type = RevitRepository.GetElement(typeId);
                    return new ParamValueViewModel(RevitParam, type);
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
                id = RevitParam.GetRevitParamElement(RevitRepository.Doc).Id;
            }
            return creator.Create(RevitParam.StorageType, id, value);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterValueProvider);
        }

        public override int GetHashCode() {
            int hashCode = 208823010;
            hashCode = hashCode * -1521134295 + EqualityComparer<RevitParam>.Default.GetHashCode(RevitParam);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public bool Equals(ParameterValueProvider other) {
            return RevitParam?.StorageType == other?.RevitParam?.StorageType
                && Name == other?.Name;
        }
    }
}
