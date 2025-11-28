namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class HasValueOperator : ComparisonOperator {
    public HasValueOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !string.IsNullOrWhiteSpace(actualValue);
    }
}
