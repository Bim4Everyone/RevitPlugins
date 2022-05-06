using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

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
