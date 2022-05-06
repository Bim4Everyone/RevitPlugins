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
        IEnumerable<RuleEvaluator> GetRuleEvaluators();
        IEnumerable<object> GetValues(IEnumerable<Category> categories, RuleEvaluator ruleEvaluator);
    }
}
