using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Set : Criterion {
        public SetEvaluator SetEvaluator { get; set; } = SetEvaluatorUtils.GetEvaluators().FirstOrDefault(item => item.Evaluator == SetEvaluators.And);
        public List<Criterion> Criteria { get; set; } = new List<Criterion>();

        public override IFilterGenerator Generate(Document doc) {
            return FilterGenerator.SetSetFilter(doc, this);
        }

        public override IEnumerable<IFilterableValueProvider> GetProviders() {
            foreach(var criterion in Criteria) {
                foreach(var provider in criterion.GetProviders()) {
                    yield return provider;
                }
            }
        }

        public override void SetRevitRepository(RevitRepository revitRepository) {
            base.SetRevitRepository(revitRepository);
            foreach(var criterion in Criteria) {
                criterion.SetRevitRepository(revitRepository);
            }
        }
    }
}
