using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal class LessOperator : ComparisonOperator {
    public LessOperator() {
    }

    public override bool Evaluate(string actualValue, string expectedValue) {
        return NamingUtils.CompareNames(actualValue, expectedValue) < 0;
    }
}
