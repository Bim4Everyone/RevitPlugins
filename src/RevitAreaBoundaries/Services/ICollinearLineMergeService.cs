using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface ICollinearLineMergeService {
    List<Curve> MergeConnectedCollinearLines(List<Curve> curves);
}
