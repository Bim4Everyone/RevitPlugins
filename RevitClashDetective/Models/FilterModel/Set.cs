using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterModel {
    internal class Set : ICriterion {
        public SetEvaluator SetEvaluator { get; set; }
        public List<ICriterion> Criteria { get; set; }

        public IFilterGenerator FilterGenerator { get; set; }

        public IFilterGenerator Generate() {
            return FilterGenerator.SetSetFilter(this);
        }
    }
}
