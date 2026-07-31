using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.Models.Configs;

public class LintelManagerConfig {
    public LintelFamilyConfig LintelFamilyConfig { get; set; }
    public OpeningFamilyConfig OpeningFamilyConfig { get; set; }
    public LintelConfigRule LintelConfigRule { get; set; }
    public RevitParam LintelFixParam { get; set; }
    public double? MinimalHeightAboveOpeningMm { get; set; }
    public IEnumerable<RevitWall> StructureWalls { get; set; }
    public ElementId PhaseId { get; set; }
}
