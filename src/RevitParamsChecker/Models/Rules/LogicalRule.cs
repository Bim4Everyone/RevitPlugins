using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitParamsChecker.Models.Rules.LogicalOperators;

namespace RevitParamsChecker.Models.Rules;

internal class LogicalRule : ValidationRule {
    public LogicalRule() {
    }

    public ValidationRule[] ChildRules { get; set; } = [];

    public LogicalOperator Operator { get; set; } = new AndOperator();

    public override EvaluationResult Evaluate(Element element) {
        if(ChildRules is null) {
            throw new InvalidOperationException($"Перед вызовом метода надо назначить {nameof(ChildRules)}");
        }

        if(Operator is null) {
            throw new InvalidOperationException($"Перед вызовом метода надо назначить {nameof(Operator)}");
        }

        var results = ChildRules.Select(r => r.Evaluate(element)).ToArray();
        if(Operator.Combine(results.Select(r => r.Success))) {
            return EvaluationResult.CreateSuccessResult();
        }

        return EvaluationResult.CreateUnsuccessResult([.. results.Where(r => !r.Success).SelectMany(r => r.Failures)]);
    }

    public override ValidationRule Copy() {
        return new LogicalRule() {
            Operator = Operator.Copy(),
            ChildRules = [..ChildRules.Select(c => c.Copy())]
        };
    }
}
