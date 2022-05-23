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
using RevitClashDetective.Models.Value;

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
                return RevitRepository.GetCollectors()
                    .SelectMany(item => GetValues(item, categoryFilter))
                    .Distinct()
                    .OrderBy(item => item.ParamValue);
            }
            return Enumerable.Empty<ParamValueViewModel>();
        }

        private IEnumerable<ParamValueViewModel> GetValues(Collector collector, ElementMulticategoryFilter categoryFilter) {
            return collector.RevitCollector.WherePasses(categoryFilter)
                       .WhereElementIsNotElementType()
                       .Select(item=>GetElementParam(collector.Document, item))
                       .Where(item => item != null && item.Value != null && item.Value as ElementId != ElementId.InvalidElementId);
        }

        private ParamValueViewModel GetElementParam(Document doc, Element item) {

            if(item.IsExistsParam(RevitParam)) {
                return new ParamValueViewModel(RevitParam, item);
            } else {
                var typeId = item.GetTypeId();
                if(typeId != null) {
                    var type = RevitRepository.GetElement(doc, typeId);
                    return new ParamValueViewModel(RevitParam, type);
                }
            }
            return null;
        }

        public FilterRule GetRule(IRevitRuleCreator creator, object value) {
            ElementId id = null;
            if(RevitParam is SystemParam systemParam) {
                id = new ElementId(systemParam.SystemParamId);

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
            hashCode = hashCode * -1521134295 + EqualityComparer<StorageType>.Default.GetHashCode(RevitParam.StorageType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public bool Equals(ParameterValueProvider other) {
            return RevitParam?.StorageType == other?.RevitParam?.StorageType
                && Name == other?.Name;
        }

        public string GetErrorText(string value) {
            switch(RevitParam.StorageType) {
                default: {
                    throw new ArgumentOutOfRangeException(nameof(RevitParam.StorageType), $"У параметра {RevitParam.Name} не определен тип данных.");
                }
                case StorageType.Integer:
                return int.TryParse(value, out int intRes) ? null : $"Значение параметра \"{Name}\" должно быть целым числом.";
                case StorageType.Double:
                return double.TryParse(value, out double doubleRes) ? null : $"Значение параметра \"{Name}\" должно быть вещественным числом.";
                case StorageType.String:
                return null;
                case StorageType.ElementId:
                return null;
            }
        }

        public ParamValue GetParamValue(string value) {
            return ParamValue.GetParamValue(RevitParam, value);
        }
    }
}
