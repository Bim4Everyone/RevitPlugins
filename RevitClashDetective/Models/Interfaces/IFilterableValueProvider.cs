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
        RevitRepository RevitRepository { get; set; }
        IFilterGenerator FilterGenerator { get; }
        string GetErrorText(string value);
        ParamValue GetParamValue(string value);
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<ParamValueViewModel> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator);
        FilterRule GetRule(Document doc, IRevitRuleCreator creator, object value);
    }
}
