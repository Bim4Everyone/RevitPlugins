using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class GreaterOrEqualOperator : ComparisonOperator {
    public GreaterOrEqualOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return NamingUtils.CompareNames(actualValue, expectedValue) >= 0;
    }

    public override bool Equals(ComparisonOperator other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return other is GreaterOrEqualOperator;
    }
}
