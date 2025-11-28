using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class EndsWithOperator : ComparisonOperator {
    public EndsWithOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return actualValue.EndsWith(expectedValue, StringComparison.OrdinalIgnoreCase);
    }
}
