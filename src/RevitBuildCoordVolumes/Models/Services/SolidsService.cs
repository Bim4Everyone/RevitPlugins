using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SolidsService {
    // Направление экструзии - вверх
    private readonly XYZ _direction = new(0, 0, 10);

    public Solid ExtrudeArea(Area area, double newPosition, double amount) {
        var listCurveLoops = new List<CurveLoop>();
        foreach(var segments in area.GetBoundarySegments(new SpatialElementBoundaryOptions())) {
            var curves = new List<Curve>();
            foreach(var segment in segments) {
                var curve = segment.GetCurve();
                var newCurve = GetTransformedCurve(curve, newPosition);
                curves.Add(newCurve);
            }
            var curveLoop = CurveLoop.Create(curves);
            listCurveLoops.Add(curveLoop);
        }
        return ExtrudeSolid(listCurveLoops, amount);
    }

    public Solid ExtrudePolygon(Polygon polygon, double newPosition, double amount) {
        var listCurveLoops = new List<CurveLoop>();
        var curves = new List<Curve>();
        foreach(var curve in polygon.Sides) {
            var newCurve = GetTransformedCurve(curve, newPosition);
            curves.Add(newCurve);
        }
        var curveLoop = CurveLoop.Create(curves);
        listCurveLoops.Add(curveLoop);
        return ExtrudeSolid(listCurveLoops, amount);
    }

    private Curve GetTransformedCurve(Curve curve, double newPosition) {
        double oldZ = curve.GetEndPoint(0).Z;
        var transform = Transform.CreateTranslation(new XYZ(0, 0, newPosition - oldZ));
        return curve.CreateTransformed(transform);
    }

    public (Solid resultSolid, List<Solid> failedSolids) UnionSolids(List<Solid> solids, DirectShape directShape, int maxTries = 20) {
        if(solids == null || solids.Count == 0) {
            return (null, null);
        }

        if(solids.Count == 1) {
            return (solids[0], new List<Solid>());
        }

        var remainingSolids = new List<Solid>(solids);
        var resultSolid = remainingSolids[0];
        remainingSolids.RemoveAt(0);

        bool optimized = true;
        int tries = 0;

        while(remainingSolids.Count > 0 && tries < maxTries) {
            optimized = false;
            for(int i = 0; i < remainingSolids.Count; ++i) {
                Solid mergedSolid = null;
                try {
                    mergedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(resultSolid, remainingSolids[i], BooleanOperationsType.Union);
                } catch {
                    continue;
                }

                if(directShape.IsValidShape([mergedSolid])) {
                    resultSolid = mergedSolid;
                    remainingSolids.RemoveAt(i);
                    optimized = true;
                    break; // После успешного объединения — начать цикл заново
                }
            }
            if(!optimized) {
                // Ни один solid не смог объединиться — можно попробовать переставить их
                tries++;
                // Например, простой вариант: перемешать список — вдруг другой порядок даст результат
                remainingSolids = remainingSolids.OrderBy(x => Guid.NewGuid()).ToList();
            }
        }

        // По завершении: resultSolid — итог объединения;
        // если remainingSolids.Count > 0, значит часть solid'ов объединить не удалось.

        return (resultSolid, remainingSolids);
    }

    private Solid ExtrudeSolid(List<CurveLoop> listCurveLoops, double amount) {
        return amount > 0 ? GeometryCreationUtilities.CreateExtrusionGeometry(listCurveLoops, _direction, amount) : null;
    }


}
