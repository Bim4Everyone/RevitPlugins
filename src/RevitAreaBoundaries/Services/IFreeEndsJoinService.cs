using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

internal interface IFreeEndsJoinService {
    List<Curve> JoinNearestFreeEndsSmart(
        List<Curve> curves,
        int maxPairsPerRun = int.MaxValue,
        bool avoidCrossings = true,
        double angleWeight = 0.35);
}
