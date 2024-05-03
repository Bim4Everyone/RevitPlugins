using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.FilterableValueProviders {
    internal class WorksetValueProvider : IFilterableValueProvider, IEquatable<WorksetValueProvider> {

        public WorksetValueProvider(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
            Name = Enum.GetName(typeof(BuiltInParameter), BuiltInParameter.ELEM_PARTITION_PARAM);
            DisplayValue = LabelUtils.GetLabelFor(BuiltInParameter.ELEM_PARTITION_PARAM);
        }

        public string Name { get; set; }

        [JsonIgnore]
        public string DisplayValue { get; set; }

        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }
#if REVIT_2020_OR_LESS
        [JsonIgnore]
        public UnitType UnitType => UnitType.UT_Number;
#else
        public ForgeTypeId UnitType => new ForgeTypeId();
#endif
        [JsonIgnore]
        public StorageType StorageType => StorageType.String;

        public bool Equals(WorksetValueProvider other) {
            return other != null &&
                   Name == other.Name;
        }

        public override bool Equals(object obj) {
            return Equals(obj as WorksetValueProvider);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public ParamValue GetElementParamValue(Element item) {
            if(item.IsExistsParam(BuiltInParameter.ELEM_PARTITION_PARAM)) {
                return new StringParamValue(item.GetParam(BuiltInParameter.ELEM_PARTITION_PARAM).AsValueString(), item.GetParam(BuiltInParameter.ELEM_PARTITION_PARAM).AsValueString());
            }
            var typeId = item.GetTypeId();
            if(typeId.IsNotNull()) {
                var type = item.Document.GetElement(typeId);
                return GetElementParamValue(type);
            }
            return null;
        }

        public ParamValue GetParamValueFormString(string value) {
            return new StringParamValue(value, value);
        }

        public FilterRule GetRule(Document doc, IVisiter visiter, ParamValue paramValue) {
            var revitParam = SystemParamsConfig.Instance.CreateRevitParam(doc, BuiltInParameter.ELEM_PARTITION_PARAM);
            return paramValue.GetFilterRule(visiter, doc, revitParam);
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


    }
}
