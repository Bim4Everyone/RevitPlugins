using System;

namespace RevitParamsChecker.Models.Rules.ComparisonOperators;

internal abstract class ComparisonOperator : IEquatable<ComparisonOperator> {
    public abstract bool Evaluate(string actualValue, string expectedValue);
    public abstract bool Equals(ComparisonOperator other);
}
