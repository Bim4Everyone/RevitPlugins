using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterGenerators {

    internal abstract class RevitFilterGenerator : IFilterGenerator {
        public ElementFilter Filter { get; protected set; }

        public abstract IFilterGenerator SetRuleFilter(Document doc, Rule rule);

        public abstract IFilterGenerator SetSetFilter(Document doc, Set set);

        protected IFilterGenerator SetRuleFilter(Document doc, Rule rule, bool inverted) {
            var ruleCreator = RuleEvaluatorUtils.GetRevitRuleCreator(rule.Evaluator.Evaluator);
            var revitRule = rule.Provider.GetRule(doc, ruleCreator, rule.Value);
            if(revitRule == null) {
                Filter = new ElementIsElementTypeFilter(inverted);
            } else {
                Filter = new ElementParameterFilter(revitRule, inverted);
            }

            return this;
        }

        private protected IEnumerable<ElementFilter> GetFilters(Document doc, Set set, RevitFilterGenerator revitFilterGenerator) {
            foreach(var criterion in set.Criteria) {
                criterion.FilterGenerator = revitFilterGenerator;
                criterion.Generate(doc);
                yield return revitFilterGenerator.Generate();
            }
        }

        public ElementFilter Generate() {
            return Filter;
        }
    }

    internal class StraightRevitFilterGenerator : RevitFilterGenerator {

        public override IFilterGenerator SetRuleFilter(Document doc, Rule rule) {
            return SetRuleFilter(doc, rule, false);
        }

        public override IFilterGenerator SetSetFilter(Document doc, Set set) {
            var filters = GetFilters(doc, set, new StraightRevitFilterGenerator()).ToList();
            var creator = SetEvaluatorUtils.GetRevitLogicalFilterCreator(set.SetEvaluator.Evaluator);
            if(filters.Count > 0) {
                Filter = creator.Create(filters);
            } else {
                Filter = new ElementIsElementTypeFilter(true);
            }
            return this;
        }
    }

    internal class InvertedRevitFilterGenerator : RevitFilterGenerator {

        public override IFilterGenerator SetRuleFilter(Document doc, Rule rule) {
            return SetRuleFilter(doc, rule, true);
        }

        public override IFilterGenerator SetSetFilter(Document doc, Set set) {
            var filters = GetFilters(doc, set, new InvertedRevitFilterGenerator()).ToList();
            var creator = SetEvaluatorUtils.GetInvertedRevitLogicalFilterCreator(set.SetEvaluator.Evaluator);
            if(filters.Count > 0) {
                Filter = creator.Create(filters);
            } else {
                Filter = new ElementIsElementTypeFilter(false);
            }
            return this;
        }
    }
}
