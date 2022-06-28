using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IFilterableValueProvider {
        string Name { get; }
        string DisplayValue { get; }
        RevitRepository RevitRepository { get; set; }
#if D2020 || R2020
        UnitType UnitType { get; }
#else
        ForgeTypeId UnitType { get; }
#endif
        StorageType StorageType { get; }
        ParamValue GetParamValueFormString(int[] categories, string value);
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<ParamValue> GetValues(Category[] categories, RuleEvaluator ruleEvaluator);
        ParamValue GetElementParamValue(int[] categories, Element item);
        FilterRule GetRule(Document doc, IVisiter visiter, ParamValue paramValue);
    }
}
