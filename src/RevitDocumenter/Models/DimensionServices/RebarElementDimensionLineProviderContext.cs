using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
internal class RebarElementDimensionLineProviderContext : IDimensionLineProviderContext {
    public RebarElementDimensionLineProviderContext(RebarElement rebar, XYZ direction) {
        Rebar = rebar;
        Direction = direction;
    }

    public RebarElement Rebar { get; set; }
    public XYZ Direction { get; set; }
}
