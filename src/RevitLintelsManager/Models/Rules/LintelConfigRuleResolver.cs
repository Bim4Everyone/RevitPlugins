using System.Collections.Generic;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleResolver {
    public bool HasError { get; set; }
    
    public IEnumerable<LintelConfigRule> LintelConfigRules { get; set; }
    
    public LintelConfigRule GetLintelConfigRule(string lintelConfigRuleName) {
        return null;
    }
}
