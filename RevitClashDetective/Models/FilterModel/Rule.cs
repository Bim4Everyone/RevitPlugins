using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Rule : ICriterion {
        public RuleEvaluator Evaluator { get; set; }
        public ParamValue Value { get; set; }
        public IFilterableValueProvider Provider { get; set; }

        public IFilterGenerator FilterGenerator { get; set; }

        public IFilterGenerator Generate() {
            return FilterGenerator.SetRuleFilter(this);
        }
    }
}
