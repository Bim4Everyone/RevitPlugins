using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class ContainsOperator : ComparisonOperator {
    public ContainsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.IndexOf(expectedValue, StringComparison.CurrentCultureIgnoreCase) > 0;
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is ContainsOperator;
    }

    public override ComparisonOperator Copy() {
        return new ContainsOperator();
    }
}
