using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface ICurveNormalizeService {
    List<Curve> ProjectCurvesToXy(List<Curve> curves);
}
