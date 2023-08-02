using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IFilterableValueProvider {
        string Name { get; }
        string DisplayValue { get; }
        RevitRepository RevitRepository { get; set; }
#if REVIT_2020_OR_LESS
        UnitType UnitType { get; }
#else
        ForgeTypeId UnitType { get; }
#endif
        StorageType StorageType { get; }
        ParamValue GetParamValueFormString(string value);
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<ParamValue> GetValues(Category[] categories, RuleEvaluator ruleEvaluator);
        ParamValue GetElementParamValue(Element item);
        FilterRule GetRule(Document doc, IVisiter visiter, ParamValue paramValue);
    }
}
