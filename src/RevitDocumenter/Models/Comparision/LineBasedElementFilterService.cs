using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Comparision;
internal class LineBasedElementFilterService {
    public List<Reference> GetGridReferencesByDirection(List<Grid> grids, XYZ direction) {
        return GetGridsByDirection(grids, direction)
            .Select(g => new Reference(g))
            .ToList();
    }

    public List<Grid> GetGridsByDirection(List<Grid> grids, XYZ direction) {
        return grids
            .Where(g => !g.IsCurved)
            .Where(g => IsGridLineParallelToDirection((Line) g.Curve, direction))
            .ToList();
    }

    private bool IsGridLineParallelToDirection(Line line, XYZ direction) {
        return line.Direction.IsAlmostEqualTo(direction) || line.Direction.IsAlmostEqualTo(direction.Negate());
    }
}
