using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

public class LintelPlaceRule {
    public string LintelPlaceRuleName { get; set; }
    public IEnumerable<LintelRule> LintelRules { get; set; }
    public IEnumerable<string> WallTypeIds { get; set; }
}
