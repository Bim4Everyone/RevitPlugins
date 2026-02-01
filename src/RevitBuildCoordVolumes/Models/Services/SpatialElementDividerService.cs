using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SpatialElementDividerService : ISpatialElementDividerService {
    public List<PolygonObject> DivideSpatialElement(SpatialElement spatialElement, double side, double angleRad) {
        var opt = new SpatialElementBoundaryOptions();
        var loops = spatialElement.GetBoundarySegments(opt);

        if(loops == null || loops.Count == 0) {
            return [];
        }

        var contour = loops[0]
            .Select(s => s.GetCurve().GetEndPoint(0))
            .ToList();

        double minX = contour.Min(p => p.X);
        double maxX = contour.Max(p => p.X);
        double minY = contour.Min(p => p.Y);
        double maxY = contour.Max(p => p.Y);

        var origin = new XYZ((minX + maxX) / 2, (minY + maxY) / 2, 0);

        var ux = new XYZ(Math.Cos(angleRad), Math.Sin(angleRad), 0);
        var uy = new XYZ(-Math.Sin(angleRad), Math.Cos(angleRad), 0);

        double halfWidth = (maxX - minX) / 2;
        double halfHeight = (maxY - minY) / 2;

        int nx = (int) (halfWidth / side) + 1;
        int ny = (int) (halfHeight / side) + 1;

        var polygons = new List<PolygonObject>();

        for(int ix = -nx; ix <= nx; ix++) {
            for(int iy = -ny; iy <= ny; iy++) {

                var basePoint = origin + ux * (ix * side) + uy * (iy * side);

                var p1 = basePoint;
                var p2 = basePoint + ux * side;
                var p3 = basePoint + ux * side + uy * side;
                var p4 = basePoint + uy * side;

                if(!GeometryUtility.IsPointInsidePolygon(p1, contour)
                    || !GeometryUtility.IsPointInsidePolygon(p2, contour)
                    || !GeometryUtility.IsPointInsidePolygon(p3, contour)
                    || !GeometryUtility.IsPointInsidePolygon(p4, contour)) {
                    continue;
                }

                var center = (p1 + p3) / 2;

                if(!GeometryUtility.IsPointInsidePolygon(center, contour)) {
                    continue;
                }

                polygons.Add(new PolygonObject {
                    Center = center,
                    Sides = [
                        Line.CreateBound(p1, p2),
                        Line.CreateBound(p2, p3),
                        Line.CreateBound(p3, p4),
                        Line.CreateBound(p4, p1)
                    ]
                });
            }
        }
        return polygons;
    }
}
