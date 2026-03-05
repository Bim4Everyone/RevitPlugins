using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Comparision;
internal class GridComparisonContext : IComparisonContext {
    public GridComparisonContext(List<Reference> rebarReferences, List<Grid> grids, XYZ direction) {
        RebarReferences = rebarReferences;
        Grids = grids;
        Direction = direction;
    }
    public List<Reference> RebarReferences { get; set; }
    public List<Grid> Grids { get; set; }
    public XYZ Direction { get; set; }
}
