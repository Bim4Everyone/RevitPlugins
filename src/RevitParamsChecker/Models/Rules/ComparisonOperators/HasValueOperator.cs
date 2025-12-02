namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class HasValueOperator : ComparisonOperator {
    public HasValueOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !string.IsNullOrWhiteSpace(actualValue);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is HasValueOperator;
    }
}
