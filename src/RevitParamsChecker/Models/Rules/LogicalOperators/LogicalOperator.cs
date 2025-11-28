using System.Collections.Generic;

namespace RevitParamsChecker.Models.Rules.LogicalOperators;

internal abstract class LogicalOperator {
    public abstract bool Combine(IEnumerable<bool> results);
}
