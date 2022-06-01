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

        public string DisplayValue { get; set; }

        public ParameterValueProvider(RevitRepository revitRepository, RevitParam revitParam, string displayValue = null) {
            RevitRepository = revitRepository;
            RevitParam = revitParam;
            DisplayValue = displayValue ?? revitParam.Name;
        }

        public IEnumerable<RuleEvaluator> GetRuleEvaluators() {
            return RuleEvaluatorUtils.GetRuleEvaluators(RevitParam.StorageType);
        }

        public IEnumerable<ParamValue> GetValues(Category[] categories, RuleEvaluator ruleEvaluator) {
            if(ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue && ruleEvaluator.Evaluator != RuleEvaluators.FilterHasValue) {
                var categoryFilter = new ElementMulticategoryFilter(categories.Select(item => item.Id).ToList());
                return RevitRepository.GetCollectors()
                    .SelectMany(item => GetValues(categories.Select(c => c.Id.IntegerValue).ToArray(), item, categoryFilter))
                    .Distinct()
                    .OrderBy(item => item);
            }
            return Enumerable.Empty<ParamValue>();
        }

        private IEnumerable<ParamValue> GetValues(int[] categories, Collector collector, ElementMulticategoryFilter categoryFilter) {
            return collector.RevitCollector.WherePasses(categoryFilter)
                       .WhereElementIsNotElementType()
                       .Select(item => GetElementParam(categories, collector.Document, item))
                       .Where(item => item != null && item.Value != null && item.Value as ElementId != ElementId.InvalidElementId);
        }

        private ParamValue GetElementParam(int[] categories, Document doc, Element item) {
            if(item.IsExistsParam(RevitParam)) {
                return ParamValue.GetParamValue(categories, RevitParam, item);
            } else {
                var typeId = item.GetTypeId();
                if(typeId != null) {
                    var type = doc.GetElement(typeId);
                    return ParamValue.GetParamValue(categories, RevitParam, type);
                }
            }
            return null;
        }

        public FilterRule GetRule(Document doc, IVisiter visiter, ParamValue paramValue) {
            return paramValue.GetFilterRule(visiter, doc, RevitParam);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterValueProvider);
        }

        public override int GetHashCode() {
            int hashCode = 208823010;
            hashCode = hashCode * -1521134295 + EqualityComparer<StorageType>.Default.GetHashCode(RevitParam.StorageType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RevitParam.Id);
            return hashCode;
        }

        public bool Equals(ParameterValueProvider other) {
            return other != null
                && RevitParam?.StorageType == other.RevitParam?.StorageType
                && Name == other.Name
                && RevitParam?.Id == other.RevitParam?.Id;
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

        public ParamValue GetParamValue(int[] categories, string value) {
            return ParamValue.GetParamValue(categories, RevitParam, value);
        }
    }
}
