using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotContainsOperator : ComparisonOperator {
    public NotContainsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) <= 0;
    }
}
