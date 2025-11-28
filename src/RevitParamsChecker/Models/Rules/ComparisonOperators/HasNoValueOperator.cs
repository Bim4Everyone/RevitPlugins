namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class HasNoValueOperator : ComparisonOperator {
    public HasNoValueOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return string.IsNullOrWhiteSpace(actualValue);
    }
}
