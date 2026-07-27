using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal interface ICellsBoundaryService {
    HashSet<CellSquare> GetCoarseCells(List<XYZ> squareVertices, List<Curve> curves);
    HashSet<Curve> GetBoundaryCurves(HashSet<CellSquare> cells, ProgressService progressService);
}
