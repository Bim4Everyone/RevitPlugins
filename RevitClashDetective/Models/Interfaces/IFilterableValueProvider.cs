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
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<ParamValueViewModel> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator);
        FilterRule GetRule(IRevitRuleCreator creator, object value);
    }
}
