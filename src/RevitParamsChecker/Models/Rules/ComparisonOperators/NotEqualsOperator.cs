using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotEqualsOperator : ComparisonOperator {
    public NotEqualsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
