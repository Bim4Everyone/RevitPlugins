using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.FilterModel {
    internal class Rule : Criterion {
        public RuleEvaluator Evaluator { get; set; }
        public ParamValue Value { get; set; }
        public IFilterableValueProvider Provider { get; set; }

        public override IFilterGenerator Generate() {
            return FilterGenerator.SetRuleFilter(this);
        }

        public override void SetRevitRepository(RevitRepository revitRepository) {
            base.SetRevitRepository(revitRepository);
            Provider.RevitRepository = revitRepository;
        }
    }
}
