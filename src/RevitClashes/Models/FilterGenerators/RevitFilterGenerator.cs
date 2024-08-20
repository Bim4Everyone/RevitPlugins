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
            var providerRule = rule.Provider.GetRule(doc, ruleCreator, rule.Value);
            var revitRule = inverted
                ? new FilterInverseRule(providerRule)
                : providerRule;
            if(revitRule == null) {
                Filter = new ElementIsElementTypeFilter(inverted);
            } else {
                // Если создавать ElementParameterFilter с параметром конструктора inverted=true,
                // то работать не будет нормально, когда нужен инвертированный фильтр.
                // Надо в параметре filterRule указывать уже инвертированное правило, созданное через FilterInverseRule.
                // https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/View-Filters-with-OR-rule-does-not-work-on-linked-models-in-Revit.html
                Filter = new ElementParameterFilter(revitRule, false);
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
