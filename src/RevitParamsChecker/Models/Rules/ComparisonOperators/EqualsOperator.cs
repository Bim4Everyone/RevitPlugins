using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class EqualsOperator : ComparisonOperator {
    public EqualsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
