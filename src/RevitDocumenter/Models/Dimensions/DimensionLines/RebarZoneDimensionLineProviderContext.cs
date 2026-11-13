using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal class RebarZoneDimensionLineProviderContext : IDimensionLineProviderContext {
    public RebarZoneDimensionLineProviderContext(Element element, XYZ direction) {
        Element = element.ThrowIfNull();
        Direction = direction.ThrowIfNull();
    }

    public Element Element { get; set; }
    public XYZ Direction { get; set; }
}
