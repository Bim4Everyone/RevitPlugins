using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitRoundingOfAreas.Models;
internal class ConfigSettings {
    public ElementId SelectedPhaseId { get; set; }
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
    public int DigitCount { get; set; }
    public List<ElementId> Levels { get; set; }
}
