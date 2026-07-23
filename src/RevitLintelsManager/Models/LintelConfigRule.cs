using System.Collections.Generic;

namespace RevitLintelsManager.Models;

public class LintelConfigRule {
    public string RuleConfigName { get; set; }
    public ICollection<LintelPlaceRule> LintelPlaceRules { get; set; }
}
