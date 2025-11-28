namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal abstract class ComparisonOperator {
    public abstract bool Evaluate(string actualValue, string expectedValue);
}
