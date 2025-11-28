using System;
using System.Collections.Generic;

using RevitParamsChecker.Models.Checks;

namespace RevitParamsChecker.Models.Results;

internal class CheckResult {
    public CheckResult(ICollection<RuleResult> ruleResults, Check checkCopy) {
        RuleResults = ruleResults ?? throw new ArgumentNullException(nameof(ruleResults));
        CheckCopy = checkCopy ?? throw new ArgumentNullException(nameof(checkCopy));
    }

    public ICollection<RuleResult> RuleResults { get; }
    public Check CheckCopy { get; }
}
