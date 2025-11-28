using System.Collections.Generic;
using System.Linq;

namespace RevitParamsChecker.Models.Rules.LogicalOperators;

internal class AndOperator : LogicalOperator {
    public AndOperator() {
    }

    public override bool Combine(IEnumerable<bool> results) {
        bool[] arr = results?.ToArray() ?? [];
        return arr.Length == 0 || arr.All(r => r == true);
    }
}
