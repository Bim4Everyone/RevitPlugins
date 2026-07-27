using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

public interface IOuterSquareService {
    List<XYZ> BuildOuterSquareVertices(IEnumerable<Curve> curves);
}
