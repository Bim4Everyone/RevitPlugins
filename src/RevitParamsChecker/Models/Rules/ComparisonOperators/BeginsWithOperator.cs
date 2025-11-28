using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class BeginsWithOperator : ComparisonOperator {
    public BeginsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.StartsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
