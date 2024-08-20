using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.FilterModel {
    internal class Rule : Criterion {
        public RuleEvaluator Evaluator { get; set; }
        public ParamValue Value { get; set; }
        public IFilterableValueProvider Provider { get; set; }

        public override IFilterGenerator Generate(Document doc) {
            return FilterGenerator.SetRuleFilter(doc, this);
        }

        public override IEnumerable<IFilterableValueProvider> GetProviders() {
            yield return Provider;
        }

        public override void SetRevitRepository(RevitRepository revitRepository) {
            base.SetRevitRepository(revitRepository);
            Provider.RevitRepository = revitRepository;
        }
    }
}
