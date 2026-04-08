using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SpatialElementDividerService : ISpatialElementDividerService {
    private readonly IContourService _contourService;

    public SpatialElementDividerService(IContourService contourService) {
        _contourService = contourService;
    }

    public List<PolygonObject> DivideSpatialElement(
        SpatialElement spatialElement,
        double side,
        double angleDeg,
        ProgressService progressService) {
        var contour = _contourService.GetOuterContour(spatialElement);
        if(contour.Count == 0) {
            return [];
        }

        var contourDots = contour
            .Select(curve => curve.GetEndPoint(0))
            .ToList();

        double minX = contourDots.Min(p => p.X);
        double maxX = contourDots.Max(p => p.X);
        double minY = contourDots.Min(p => p.Y);
        double maxY = contourDots.Max(p => p.Y);

        var origin = new XYZ((minX + maxX) / 2, (minY + maxY) / 2, 0);

        var ux = new XYZ(Math.Cos(angleDeg), Math.Sin(angleDeg), 0);
        var uy = new XYZ(-Math.Sin(angleDeg), Math.Cos(angleDeg), 0);

        double halfWidth = (maxX - minX) / 2;
        double halfHeight = (maxY - minY) / 2;

        int nx = (int) (halfWidth / side) + 1;
        int ny = (int) (halfHeight / side) + 1;

        progressService?.BeginStage(ProgressType.DivideSpatial);
        int total = (2 * nx + 1) * (2 * ny + 1);
        int processed = 0;
        int reported = 0;
        var polygons = new List<PolygonObject>();
        for(int ix = -nx; ix <= nx; ix++) {
            for(int iy = -ny; iy <= ny; iy++) {
                progressService?.CancellationToken.ThrowIfCancellationRequested();

                var basePoint = origin + ux * (ix * side) + uy * (iy * side);

                var p1 = basePoint;
                var p2 = basePoint + ux * side;
                var p3 = basePoint + ux * side + uy * side;
                var p4 = basePoint + uy * side;

                if(!GeometryUtility.IsPointInsidePolygon(p1, contourDots)
                    || !GeometryUtility.IsPointInsidePolygon(p2, contourDots)
                    || !GeometryUtility.IsPointInsidePolygon(p3, contourDots)
                    || !GeometryUtility.IsPointInsidePolygon(p4, contourDots)) {
                    continue;
                }

                var center = (p1 + p3) / 2;

                if(!GeometryUtility.IsPointInsidePolygon(center, contourDots)) {
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
                processed++;
                int current = processed * 100 / total;
                if(current > 100) {
                    current = 100;
                }
                if(current > reported) {
                    reported = current;
                    progressService?.ProgressCount?.Report(reported);
                }
            }
        }
        return polygons;
    }
}
