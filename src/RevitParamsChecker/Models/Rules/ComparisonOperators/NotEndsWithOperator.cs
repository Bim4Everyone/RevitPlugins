using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotEndsWithOperator : ComparisonOperator {
    public NotEndsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !actualValue.EndsWith(expectedValue, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is NotEndsWithOperator;
    }

    public override ComparisonOperator Copy() {
        return new NotEndsWithOperator();
    }
}
