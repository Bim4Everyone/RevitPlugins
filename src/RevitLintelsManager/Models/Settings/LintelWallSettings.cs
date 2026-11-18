using System.Collections.Generic;

namespace RevitLintelsManager.Models.Settings;

public class LintelWallSettings {
    public string RuleConfigName { get; set; }
    public string LintelPlaceRuleName { get; set; }
    public IEnumerable<string> WallTypeNames { get; set; } = [];
}
