using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.ViewServices;
internal class ViewPreparerOption {
    public double MappingStepInMm { get; set; }
    public double MappingStepInFeet { get; set; }
    public Color ColorForAnchorLines { get; set; }
    public int WeightForAnchorLines { get; set; }
}
