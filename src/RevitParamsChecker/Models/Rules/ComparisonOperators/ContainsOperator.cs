using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class ContainsOperator : ComparisonOperator {
    public ContainsOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) > 0;
    }
}
