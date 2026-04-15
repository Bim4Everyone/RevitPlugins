using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.ViewServices;
internal class ViewPreparerOption {
    public double MappingStepInFeet { get; set; }
    public Color ColorForAnchorLines { get; set; }
    public int WeightForAnchorLines { get; set; }
}
