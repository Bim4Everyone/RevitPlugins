using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models.Settings;

public class LintelManagerSettings {
    public LintelFamilySettings LintelFamilySettings { get; set; }
    public OpeningFamilySettings OpeningFamilySettings { get; set; }
    public string LintelConfigRuleName { get; set; }
    public string LintelFixParamName { get; set; }
    public double MinimalHeightAboveOpeningMm { get; set; }
    public IEnumerable<string> StructureWallTypeNames { get; set; }
    public string PhaseName { get; set; }
    
}
