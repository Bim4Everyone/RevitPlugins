using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotEqualsOperator : ComparisonOperator {
    public NotEqualsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !actualValue.Equals(expectedValue, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is NotEqualsOperator;
    }

    public override ComparisonOperator Copy() {
        return new NotEqualsOperator();
    }
}
