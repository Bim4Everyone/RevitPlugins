using System.Collections.Generic;

using pyRevitLabs.Json;

namespace RevitLintelsManager.Models;

public class LintelPlaceRule {
    public string LintelPlaceRuleName { get; set; }
    public IEnumerable<LintelRule> LintelRules { get; set; }

    [JsonIgnore]
    public IEnumerable<string> WallTypeNames { get; set; } = [];
}
