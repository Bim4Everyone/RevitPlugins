using System.Collections.Generic;

namespace RevitLintelsManager.Models;

public class LintelConfigRule {
    public string RuleConfigName { get; set; }
    public IEnumerable<LintelPlaceRule> LintelPlaceRules { get; set; }
}
