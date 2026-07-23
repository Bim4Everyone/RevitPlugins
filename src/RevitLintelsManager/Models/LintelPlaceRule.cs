using System.Collections.Generic;

namespace RevitLintelsManager.Models;

public class LintelPlaceRule {
    public string LintelPlaceRuleName { get; set; }
    public ICollection<LintelRule> LintelRules { get; set; }
}
