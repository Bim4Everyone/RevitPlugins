using System.Collections.Generic;

namespace RevitLintelsManager.Models.Settings;

public class OpeningFamilySettings {
    public IEnumerable<string> FamilyNames { get; set; }
    public string LintelOpeningHeight { get; set; }
    public string LintelOpeningWidth { get; set; }
}
