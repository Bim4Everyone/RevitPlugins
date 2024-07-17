using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Visiter;

namespace RevitClashDetective.Models.Evaluators {
    internal class RuleEvaluatorUtils {
        public static IEnumerable<RuleEvaluator> GetRuleEvaluators(StorageType storageType) {
            if(storageType == StorageType.String) {
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringEquals, Message = "равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNotEquals, Message = "не равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringGreater, Message = "больше" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringGreaterOrEqual, Message = "больше или равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringLess, Message = "меньше" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringLessOrEqual, Message = "меньше или равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringContains, Message = "содержит" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotContains, Message = "не содержит" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringBeginsWith, Message = "начинается с" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotBeginsWith, Message = "не начинается с" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringEndsWith, Message = "заканчивается на" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotEndsWith, Message = "не заканчивается на" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasValue, Message = "имеет значение" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasNoValue, Message = "без значения" };

            } else if(storageType == StorageType.ElementId) {
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringEquals, Message = "равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNotEquals, Message = "не равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasValue, Message = "имеет значение" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasNoValue, Message = "без значения" };

                // TODO для добавления этих вариантов нужно реализовать логику получения фильтров в RevitClashDetective.Models.Value.ElementIdParamValue.GetFilterRule(...)
                // сейчас проверки с этими критериями не будут возвращать ни один элемент
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringContains, Message = "содержит" };
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotContains, Message = "не содержит" };
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringBeginsWith, Message = "начинается с" };
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotBeginsWith, Message = "не начинается с" };
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringEndsWith, Message = "заканчивается на" };
                //yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterStringNotEndsWith, Message = "не заканчивается на" };
            } else {
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNumericEquals, Message = "равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNotEquals, Message = "не равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNumericGreater, Message = "больше" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNumericGreaterOrEqual, Message = "больше или равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNumericLess, Message = "меньше" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterNumericLessOrEqual, Message = "меньше или равно" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasValue, Message = "имеет значение" };
                yield return new RuleEvaluator() { Evaluator = RuleEvaluators.FilterHasNoValue, Message = "без значения" };
            }
        }

        public static IVisiter GetRevitRuleCreator(RuleEvaluators ruleEvaluator) {
            _evaluatorDictionary.TryGetValue(ruleEvaluator, out IVisiter visiter);
            return visiter;
        }


        private static readonly Dictionary<RuleEvaluators, IVisiter> _evaluatorDictionary = new Dictionary<RuleEvaluators, IVisiter>() {
            {RuleEvaluators.FilterStringBeginsWith,new BeginsWithVisister() },
            {RuleEvaluators.FilterStringNotBeginsWith,new NotBeginsWithVisister() },
            {RuleEvaluators.FilterStringContains,new ContainsVisiter() },
            {RuleEvaluators.FilterStringNotContains,new NotContainsVisister() },
            {RuleEvaluators.FilterStringEndsWith,new EndsWithVisister() },
            {RuleEvaluators.FilterStringNotEndsWith,new NotEndsWithVisister() },
            {RuleEvaluators.FilterStringEquals,new EqualsVisiter() },
            {RuleEvaluators.FilterStringGreater,new GreaterVisister() },
            {RuleEvaluators.FilterStringGreaterOrEqual,new GreaterOrEqualVisister() },
            {RuleEvaluators.FilterStringLess,new LessVisister() },
            {RuleEvaluators.FilterStringLessOrEqual,new LessOrEqualVisister() },
            {RuleEvaluators.FilterNumericEquals,new EqualsVisiter() },
            {RuleEvaluators.FilterNumericGreater,new GreaterVisister() },
            {RuleEvaluators.FilterNumericGreaterOrEqual,new GreaterOrEqualVisister() },
            {RuleEvaluators.FilterNumericLess,new LessVisister() },
            {RuleEvaluators.FilterNumericLessOrEqual,new LessOrEqualVisister() },
            {RuleEvaluators.FilterHasNoValue,new HasNoValueVisister() },
            {RuleEvaluators.FilterHasValue,new HasValueVisister() },
            {RuleEvaluators.FilterNotEquals,new NotEqualsVisister() },
        };
    }

    internal enum RuleEvaluators {
        FilterStringBeginsWith,
        FilterStringNotBeginsWith,
        FilterStringContains,
        FilterStringNotContains,
        FilterStringEndsWith,
        FilterStringNotEndsWith,
        FilterStringEquals,
        FilterStringGreater,
        FilterStringGreaterOrEqual,
        FilterStringLess,
        FilterStringLessOrEqual,
        FilterNumericEquals,
        FilterNumericGreater,
        FilterNumericGreaterOrEqual,
        FilterNumericLess,
        FilterNumericLessOrEqual,
        FilterHasNoValue,
        FilterHasValue,
        FilterNotEquals
    }

    internal class RuleEvaluator {
        public string Message { get; set; }
        public RuleEvaluators Evaluator { get; set; }
    }
}
