using System.Collections.Generic;

using RevitClashDetective.Models.FilterCreators.RevitFilterCreators;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Evaluators {
    internal class SetEvaluatorUtils {
        public static IEnumerable<SetEvaluator> GetEvaluators() {
            yield return new SetEvaluator() { Evaluator = SetEvaluators.And, Message = "И" };
            yield return new SetEvaluator() { Evaluator = SetEvaluators.Or, Message = "ИЛИ" };
        }

        public static IRevitLogicalFilterCreator GetRevitLogicalFilterCreator(SetEvaluators evaluator) {
            _evaluatorDictionary.TryGetValue(evaluator, out IRevitLogicalFilterCreator result);
            return result;
        }

        public static IRevitLogicalFilterCreator GetInvertedRevitLogicalFilterCreator(SetEvaluators evaluator) {
            _invertedEvaluatorDictionary.TryGetValue(evaluator, out IRevitLogicalFilterCreator result);
            return result;
        }

        private static readonly Dictionary<SetEvaluators, IRevitLogicalFilterCreator> _evaluatorDictionary = new Dictionary<SetEvaluators, IRevitLogicalFilterCreator>() {
            {SetEvaluators.And, new RevitLogicalAndFilterCreator() },
            {SetEvaluators.Or, new RevitLogicalOrFilterCreator() },
        };

        private static readonly Dictionary<SetEvaluators, IRevitLogicalFilterCreator> _invertedEvaluatorDictionary = new Dictionary<SetEvaluators, IRevitLogicalFilterCreator>() {
            {SetEvaluators.And, new RevitLogicalOrFilterCreator() },
            {SetEvaluators.Or, new RevitLogicalAndFilterCreator() },
        };
    }

    internal class SetEvaluator {
        public SetEvaluators Evaluator { get; set; }
        public string Message { get; set; }
    }

    internal enum SetEvaluators {
        And,
        Or
    }
}
