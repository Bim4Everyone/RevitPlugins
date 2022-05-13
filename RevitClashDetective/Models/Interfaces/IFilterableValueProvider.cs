using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.Models.Interfaces {
    internal interface IFilterableValueProvider {
        string Name { get; }
        IFilterGenerator FilterGenerator { get; }
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<ParamValue> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator);
        FilterRule GetRule(IRevitRuleCreator creator, object value);
    }
}
