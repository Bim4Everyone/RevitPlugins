using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface ICurveDividerService {
    List<Curve> DivideToShortCurves(List<Curve> curves);
    List<Curve> SplitCurvesAtIntersections(List<Curve> curves);
}
