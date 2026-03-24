using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
internal class ReferenceToGridsCollectorContext : IReferenceCollectorContext {
    public ReferenceToGridsCollectorContext(List<Reference> elementReferences, List<Grid> grids, XYZ direction) {
        ElementReferences = elementReferences;
        Grids = grids;
        Direction = direction;
    }
    public List<Reference> ElementReferences { get; set; }
    public List<Grid> Grids { get; set; }
    public XYZ Direction { get; set; }
}
