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

    public override bool Evaluate(Element element) {
        if(ChildRules is null) {
            throw new InvalidOperationException($"Перед вызовом метода надо назначить {nameof(ChildRules)}");
        }

        if(Operator is null) {
            throw new InvalidOperationException($"Перед вызовом метода надо назначить {nameof(Operator)}");
        }

        return Operator.Combine(ChildRules.Select(r => r.Evaluate(element)));
    }

    public override ValidationRule Copy() {
        return new LogicalRule() {
            Operator = Operator.Copy(),
            ChildRules = [..ChildRules.Select(c => c.Copy())]
        };
    }
}
