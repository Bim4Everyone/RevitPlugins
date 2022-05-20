using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Set : Criterion {
        public SetEvaluator SetEvaluator { get; set; }
        public List<Criterion> Criteria { get; set; }

        public override IFilterGenerator Generate() {
            return FilterGenerator.SetSetFilter(this);
        }
        public override void SetRevitRepository(RevitRepository revitRepository) {
            base.SetRevitRepository(revitRepository);
            foreach(var criterion in Criteria) {
                criterion.SetRevitRepository(revitRepository);
            }
        }
    }
}
