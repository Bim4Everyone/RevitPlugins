using System;
using System.Collections.Generic;

namespace RevitParamsChecker.Models.Rules.LogicalOperators;

internal abstract class LogicalOperator : IEquatable<LogicalOperator> {
    public abstract bool Combine(IEnumerable<bool> results);
    public abstract bool Equals(LogicalOperator other);
}
