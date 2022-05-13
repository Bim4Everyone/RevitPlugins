using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterGenerators {
    internal class RevitFilterGenerator : IFilterGenerator {

        public ElementFilter Filter { get; private set; }

        public IFilterGenerator SetRuleFilter(Rule rule) {
            var ruleCreator = RuleEvaluatorUtils.GetRevitRuleCreator(rule.Evaluator.Evaluator);
            var revitRule = rule.Provider.GetRule(ruleCreator, rule.Value.Value);
            Filter = new ElementParameterFilter(revitRule, false);
            return this;
        }

        public IFilterGenerator SetSetFilter(Set set) {
            var filters = new List<ElementFilter>();
            foreach(var criterion in set.Criteria) {
                var revitFilterGenerator = new RevitFilterGenerator();
                criterion.FilterGenerator = revitFilterGenerator;
                criterion.Generate();
                filters.Add(revitFilterGenerator.Generate());
            }
            var creator = SetEvaluatorUtils.GetRevitLogicalFilterCreator(set.SetEvaluator.Evaluator);
            Filter = creator.Create(filters);
            return this;
        }

        public ElementFilter Generate() {
            return Filter;
        }
    }
}
