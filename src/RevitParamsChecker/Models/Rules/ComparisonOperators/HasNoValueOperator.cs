namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class HasNoValueOperator : ComparisonOperator {
    public HasNoValueOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return string.IsNullOrWhiteSpace(actualValue);
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is HasNoValueOperator;
    }
}
