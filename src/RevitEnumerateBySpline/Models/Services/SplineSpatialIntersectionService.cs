using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Находит первую точку линии внутри Solid каждого выбранного помещения.
/// </summary>
internal sealed class SplineSpatialIntersectionService : ISplineSpatialIntersectionService {
    private static readonly double _probeLength =
        UnitUtils.ConvertToInternalUnits(100, UnitTypeId.Millimeters);

    public IReadOnlyList<SpatialPointIntersection> FindIntersections(
        List<SpatialModel?> spatialModels,
        IReadOnlyList<SplinePoint> points) {

        var intersections = new List<SpatialPointIntersection>();
        if(points.Count == 0) {
            return intersections;
        }

        var firstRoom = spatialModels.Find(model => model?.SpatialElement is Room)?.SpatialElement;
        if(firstRoom is null) {
            return intersections;
        }

        using var calculator = new SpatialElementGeometryCalculator(firstRoom.Document);
        using var options = new SolidCurveIntersectionOptions();

        foreach(var spatialModel in spatialModels) {
            if(spatialModel?.SpatialElement is not Room room
               || !SpatialElementGeometryCalculator.CanCalculateGeometry(room)) {
                continue;
            }

            Solid solid = calculator.CalculateSpatialElementGeometry(room).GetGeometry();
            if(solid.Volume <= 0) {
                continue;
            }

            BoundingBoxXYZ box = solid.GetBoundingBox();
            XYZ center = box.Transform.OfPoint((box.Min + box.Max).Multiply(0.5));
            BoundingBoxXYZ? roomBox = room.get_BoundingBox(null);

            foreach(var point in points) {
                if(roomBox is not null && IsOutsideBoundingBox(point.Position, roomBox)) {
                    continue;
                }

                Line probe = CreateProbe(point.Position, center.Z);
                using SolidCurveIntersection intersection = solid.IntersectWithCurve(probe, options);
                if(intersection.SegmentCount == 0) {
                    continue;
                }

                intersections.Add(new SpatialPointIntersection(spatialModel, point));
                break;
            }
        }

        return intersections;
    }

    // Проверяет, лежит ли точка вне габаритов помещения в плане.
    private static bool IsOutsideBoundingBox(XYZ point, BoundingBoxXYZ box) {
        return point.X < box.Min.X || point.X > box.Max.X
            || point.Y < box.Min.Y || point.Y > box.Max.Y;
    }

    // Создаёт короткую вертикальную линию на средней высоте помещения.
    private static Line CreateProbe(XYZ point, double centerZ) {
        double halfLength = _probeLength / 2;
        return Line.CreateBound(
            new XYZ(point.X, point.Y, centerZ - halfLength),
            new XYZ(point.X, point.Y, centerZ + halfLength));
    }
}
