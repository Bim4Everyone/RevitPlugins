using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
internal class ReferenceToGridsCollectorContext : IReferenceCollectorContext {
    public ReferenceToGridsCollectorContext(
        List<Reference> elementReferences,
        List<Grid> grids,
        XYZ direction,
        double minDimension) {
        ElementReferences = [.. elementReferences.ThrowIfNullOrEmpty()];
        Grids = grids.ThrowIfNull();
        Direction = direction.ThrowIfNull();
        MinDimension = minDimension;
    }
    public List<Reference> ElementReferences { get; set; }
    public List<Grid> Grids { get; set; }
    public XYZ Direction { get; set; }
    public double MinDimension { get; set; }
}
