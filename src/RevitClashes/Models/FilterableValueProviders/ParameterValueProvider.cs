using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Utils;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class ParameterValueProvider : IFilterableValueProvider, IEquatable<ParameterValueProvider> {

        public RevitParam RevitParam { get; set; }

        [JsonIgnore]
        public string Name => RevitParam.Name;

        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }

        [JsonIgnore]
        public string DisplayValue { get; set; }

#if REVIT_2020_OR_LESS
        [JsonIgnore]
        public UnitType UnitType => RevitParam.UnitType;
#else
        [JsonIgnore]
        public ForgeTypeId UnitType => RevitParam.UnitType;

#endif
        [JsonIgnore]
        public StorageType StorageType => RevitParam.StorageType;

        public ParameterValueProvider(RevitRepository revitRepository, RevitParam revitParam, string displayValue = null) {
            RevitRepository = revitRepository;
            RevitParam = revitParam;
            DisplayValue = displayValue ?? revitParam.Name;
        }

        public IEnumerable<RuleEvaluator> GetRuleEvaluators() {
            return RuleEvaluatorUtils.GetRuleEvaluators(StorageType);
        }

        public IEnumerable<ParamValue> GetValues(Category[] categories, RuleEvaluator ruleEvaluator) {
            if(ruleEvaluator.Evaluator != RuleEvaluators.FilterHasNoValue && ruleEvaluator.Evaluator != RuleEvaluators.FilterHasValue) {
                var categoryFilter = new ElementMulticategoryFilter(categories.Select(item => item.Id).ToList());
                return RevitRepository.GetCollectors()
                    .SelectMany(item => GetValues(item, categoryFilter))
                    .Distinct()
                    .OrderBy(item => item);
            }
            return Enumerable.Empty<ParamValue>();
        }

        private IEnumerable<ParamValue> GetValues(Collector collector, ElementMulticategoryFilter categoryFilter) {
            return collector.RevitCollector
                        .WhereElementIsNotElementType()
                        .WherePasses(categoryFilter)
                        .Where(item => item != null)
                        .Select(item => GetElementParamValue(item))
                        .Where(item => item != null && item.Value != null && item.Value as ElementId != ElementId.InvalidElementId);
        }

        public ParamValue GetElementParamValue(Element item) {
            if(item.IsExistsParam(RevitParam)) {
                return ParamValue.GetParamValue(RevitParam, item);
            } else {
                var typeId = item.GetTypeId();
                if(typeId != null) {
                    var type = item.Document.GetElement(typeId);
                    if(type.IsExistsParam(RevitParam)) {
                        return ParamValue.GetParamValue(RevitParam, type);
                    }
                }
            }
            return null;
        }

        public FilterRule GetRule(Document doc, IVisiter visiter, ParamValue paramValue) {
            return paramValue.GetFilterRule(visiter, doc, RevitParam);
        }

        public ParamValue GetParamValueFormString(string value) {
            if(RevitParam.StorageType == StorageType.Double && value != null) {
                if(DoubleValueParser.TryParse(value, UnitType, out double res)) {
                    return ParamValue.GetParamValue(RevitParam, res.ToString(), value);
                }
            }
            return ParamValue.GetParamValue(RevitParam, value, value);
        }


        public override bool Equals(object obj) {
            return Equals(obj as ParameterValueProvider);
        }

        public override int GetHashCode() {
            int hashCode = 208823010;
            hashCode = hashCode * -1521134295 + EqualityComparer<StorageType>.Default.GetHashCode(StorageType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RevitParam.Id);
            return hashCode;
        }

        public bool Equals(ParameterValueProvider other) {
            return other != null
                && StorageType == other.StorageType
                && Name == other.Name
                && RevitParam?.Id == other.RevitParam?.Id;
        }
    }
}
