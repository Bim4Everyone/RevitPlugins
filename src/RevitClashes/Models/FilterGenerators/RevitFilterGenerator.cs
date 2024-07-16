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

    /// <summary>
    /// Класс для генерации фильтров. Использовать только для последующего использования полученных фильтров в настройках графики на видах
    /// </summary>
    internal class InvertedVisibilityRevitFilterGenerator : InvertedRevitFilterGenerator {
        public override IFilterGenerator SetRuleFilter(Document doc, Rule rule) {
            // Нельзя просто так взять и создать инвертированный фильтр через методы ревита.
            // Инвертированные фильтры, которые генерирует ревит, работают как надо только при фильтрации элементов в FilteredElementCollector.
            // Если использовать эти полученные фильтры для настроек графики на виде, элементы из связей могут не попадать в эти фильтры.
            // Проблема заключается в условиях "ИЛИ", которые работают со связями с ошибками. Статья с похожей проблемой:
            // https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/View-Filters-with-OR-rule-does-not-work-on-linked-models-in-Revit.html
            // Если менять условия типа "не содержит" на обратное "содержит", то это решает проблему фильтрации элементов на виде. По крайней мере в протестированных случаях.
            // Но при этом полученный ElementFilter в фильтре по элементам FilteredElementCollector работает неправильно.
            // По итогу мы имеем 2 разных алгоритма для генерации инвертированного фильтра:
            // через InvertedRevitFilterGenerator для генерации инвертированных фильтров для использования в FilterdElementCollector
            // и через InvertedVisibilityRevitFilterGenerator для генерации инвертированных фильтров для использования в настройках графики на виде.
            var ruleCreator = RuleEvaluatorUtils.GetRevitRuleCreator(rule.Evaluator.Evaluator, true);
            var revitRule = rule.Provider.GetRule(doc, ruleCreator, rule.Value);
            if(revitRule == null) {
                Filter = new ElementIsElementTypeFilter(true);
            } else {
                Filter = new ElementParameterFilter(revitRule, false);
            }

            return this;
        }

        public override IFilterGenerator SetSetFilter(Document doc, Set set) {
            var filters = GetFilters(doc, set, new InvertedVisibilityRevitFilterGenerator()).ToList();
            var creator = SetEvaluatorUtils.GetRevitLogicalFilterCreator(SetEvaluators.And);
            if(filters.Count > 0) {
                Filter = creator.Create(filters);
            } else {
                Filter = new ElementIsElementTypeFilter(false);
            }
            return this;
        }
    }
}
