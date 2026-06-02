using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitRoundingOfAreas.Models;
internal class ConfigSettings {
    public ElementId SelectedPhaseId { get; set; }
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
    public int Accuracy { get; set; }
}
