using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class BeginsWithOperator : ComparisonOperator {
    public BeginsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is BeginsWithOperator;
    }
}
