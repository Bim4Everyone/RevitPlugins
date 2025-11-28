using System;
using System.Collections.Generic;

using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.Models.Results;

internal class RuleResult {
    public RuleResult(ICollection<ElementResult> elementResults, Rule ruleCopy) {
        ElementResults = elementResults ?? throw new ArgumentNullException(nameof(elementResults));
        RuleCopy = ruleCopy ?? throw new ArgumentNullException(nameof(ruleCopy));
    }

    public ICollection<ElementResult> ElementResults { get; }
    public Rule RuleCopy { get; }
}
