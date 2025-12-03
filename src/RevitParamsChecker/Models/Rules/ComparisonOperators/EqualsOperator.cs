using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class EqualsOperator : ComparisonOperator {
    public EqualsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is EqualsOperator;
    }

    public override ComparisonOperator Copy() {
        return new EqualsOperator();
    }
}
