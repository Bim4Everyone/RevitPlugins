using System;
using System.Collections.Generic;

namespace RevitParamsChecker.Models.Rules;

internal class RulesChangedEventArgs : EventArgs {
    public RulesChangedEventArgs(ICollection<Rule> oldRules, ICollection<Rule> newRules) {
        OldRules = oldRules ?? throw new ArgumentNullException(nameof(oldRules));
        NewRules = newRules ?? throw new ArgumentNullException(nameof(newRules));
    }

    public ICollection<Rule> OldRules { get; }
    public ICollection<Rule> NewRules { get; }
}
