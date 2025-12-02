using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotContainsOperator : ComparisonOperator {
    public NotContainsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) <= 0;
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is NotContainsOperator;
    }
}
