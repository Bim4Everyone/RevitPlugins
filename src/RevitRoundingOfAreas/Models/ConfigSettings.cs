using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRoundingOfAreas.Models.Enums;

namespace RevitRoundingOfAreas.Models;
internal class ConfigSettings {
    public ElementsProviderType ElementsProviderType { get; set; }
    public ElementId SelectedPhaseId { get; set; }
    public RevitParam SourceParam { get; set; }
    public RevitParam TargetParam { get; set; }
    public int DigitCount { get; set; }
}
