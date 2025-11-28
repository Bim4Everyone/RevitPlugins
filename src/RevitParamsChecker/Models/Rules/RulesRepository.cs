using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitParamsChecker.Models.Rules;

internal class RulesRepository {
    private readonly RulesConfig _rulesConfig;

    public RulesRepository(RulesConfig rulesConfig) {
        _rulesConfig = rulesConfig ?? throw new ArgumentNullException(nameof(rulesConfig));
    }

    public event EventHandler<RulesChangedEventArgs> RulesChanged;

    public void SetRules(ICollection<Rule> newRules) {
        var oldRules = _rulesConfig.Rules;
        _rulesConfig.Rules = newRules?.ToArray() ?? throw new ArgumentNullException(nameof(newRules));
        _rulesConfig.SaveProjectConfig();
        RulesChanged?.Invoke(this, new RulesChangedEventArgs(oldRules, newRules));
    }

    public ICollection<Rule> GetRules() {
        return new ReadOnlyCollection<Rule>(_rulesConfig.Rules);
    }

    public Rule GetRule(string ruleName) {
        return _rulesConfig.Rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
    }
}
