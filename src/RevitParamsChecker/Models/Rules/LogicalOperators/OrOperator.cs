using System.Collections.Generic;
using System.Linq;

namespace RevitParamsChecker.Models.Rules.LogicalOperators;

internal class OrOperator : LogicalOperator {
    public OrOperator() {
    }

    public override bool Combine(IEnumerable<bool> results) {
        bool[] arr = results?.ToArray() ?? [];
        return arr.Length == 0 || arr.Any(r => r == true);
    }
}
