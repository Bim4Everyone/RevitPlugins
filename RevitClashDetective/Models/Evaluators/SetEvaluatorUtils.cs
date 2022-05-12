using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitClashDetective.Models.Evaluators {
    internal class SetEvaluatorUtils {
        public static IEnumerable<SetEvaluator> GetEvaluators() {
            yield return new SetEvaluator() { Evaluator = SetEvaluators.And, Message = "И" };
            yield return new SetEvaluator() { Evaluator = SetEvaluators.Or, Message = "ИЛИ" };
        }
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
