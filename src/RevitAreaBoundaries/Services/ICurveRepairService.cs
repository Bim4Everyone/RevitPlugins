using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface ICurveRepairService {
    List<Curve> CleanDuplicateCurves(List<Curve> curves);
    List<Curve> RepairContour(List<Curve> curves);
    List<Curve> GetCurvesConnectedByBothEnds(List<Curve> curves);
}
