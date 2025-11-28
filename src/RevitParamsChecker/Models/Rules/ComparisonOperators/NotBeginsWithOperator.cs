using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class NotBeginsWithOperator : ComparisonOperator {
    public NotBeginsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return !actualValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
