using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SpatialElementDividerService : ISpatialElementDividerService {

    public List<Polygon> DivideSpatialElement(SpatialElement spatialElement, double side) {
        var opt = new SpatialElementBoundaryOptions();
        var loops = spatialElement.GetBoundarySegments(opt);
        if(loops == null || loops.Count == 0) {
            return [];
        }

        var segs = loops[0];
        var contour = segs
            .Select(s => s.GetCurve().GetEndPoint(0))
            .ToList();

        double minX = contour.Min(p => p.X);
        double maxX = contour.Max(p => p.X);
        double minY = contour.Min(p => p.Y);
        double maxY = contour.Max(p => p.Y);

        var polygons = new List<Polygon>();

        for(double x = minX; x + side <= maxX; x += side) {
            for(double y = minY; y + side <= maxY; y += side) {
                var p1 = new XYZ(x, y, 0);
                var p2 = new XYZ(x + side, y, 0);
                var p3 = new XYZ(x + side, y + side, 0);
                var p4 = new XYZ(x, y + side, 0);

                // Проверяем, что ВСЕ вершины внутри контура
                if(!GeometryUtility.IsPointInsidePolygon(p1, contour) ||
                    !GeometryUtility.IsPointInsidePolygon(p2, contour) ||
                    !GeometryUtility.IsPointInsidePolygon(p3, contour) ||
                    !GeometryUtility.IsPointInsidePolygon(p4, contour)) {
                    continue;
                }

                // (опционально) дополнительная проверка центра
                var center = new XYZ(
                    (p1.X + p3.X) / 2,
                    (p1.Y + p3.Y) / 2,
                    0);

                if(!GeometryUtility.IsPointInsidePolygon(center, contour)) {
                    continue;
                }

                var sides = new List<Line> {
                    Line.CreateBound(p1, p2),
                    Line.CreateBound(p2, p3),
                    Line.CreateBound(p3, p4),
                    Line.CreateBound(p4, p1)
                }
                .ToList();

                if(sides.Count == 4) {
                    polygons.Add(new Polygon {
                        Sides = sides,
                        Center = center,
                        Guid = Guid.NewGuid()
                    });
                }
            }
        }

        return polygons;
    }
}
