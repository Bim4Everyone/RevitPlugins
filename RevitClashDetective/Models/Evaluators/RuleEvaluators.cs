using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.RuleCreators.RevitRuleCreators;

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

        public static IRevitRuleCreator GetRevitRuleCreator(RuleEvaluators ruleEvaluator) {
            _evaluatorDictionary.TryGetValue(ruleEvaluator, out IRevitRuleCreator creator);
            return creator;
        }

        private static Dictionary<RuleEvaluators, IRevitRuleCreator> _evaluatorDictionary = new Dictionary<RuleEvaluators, IRevitRuleCreator>() {
            {RuleEvaluators.FilterStringBeginsWith,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringNotBeginsWith,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringContains,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringNotContains,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringEndsWith,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringNotEndsWith,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringEquals,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringGreater,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringGreaterOrEqual,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringLess,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterStringLessOrEqual,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNumericEquals,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNumericGreater,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNumericGreaterOrEqual,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNumericLess,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNumericLessOrEqual,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterHasNoValue,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterHasValue,new RevitEqualsRuleCreator() },
            {RuleEvaluators.FilterNotEquals,new RevitEqualsRuleCreator() },
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
