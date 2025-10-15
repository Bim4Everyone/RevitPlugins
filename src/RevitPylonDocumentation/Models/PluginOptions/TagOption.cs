using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.PluginOptions;
internal class TagOption {
    public XYZ BodyPoint { get; set; }
    public FamilySymbol TagSymbol { get; set; }
    public double TagLength { get; set; }
    public string TopText { get; set; }
    public string BottomText { get; set; }
}
