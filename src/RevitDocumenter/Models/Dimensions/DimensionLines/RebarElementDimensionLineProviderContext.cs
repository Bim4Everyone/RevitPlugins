using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal class RebarElementDimensionLineProviderContext : IDimensionLineProviderContext {
    public RebarElementDimensionLineProviderContext(RebarElement rebar, XYZ direction) {
        Rebar = rebar.ThrowIfNull();
        Direction = direction.ThrowIfNull();
    }

    public RebarElement Rebar { get; set; }
    public XYZ Direction { get; set; }
}
