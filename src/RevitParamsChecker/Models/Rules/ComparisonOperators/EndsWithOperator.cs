using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class EndsWithOperator : ComparisonOperator {
    public EndsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.EndsWith(expectedValue, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is EndsWithOperator;
    }

    public override ComparisonOperator Copy() {
        return new EndsWithOperator();
    }
}
