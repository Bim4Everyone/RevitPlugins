using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotEndsWithOperator : ComparisonOperator {
    public NotEndsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !actualValue.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
