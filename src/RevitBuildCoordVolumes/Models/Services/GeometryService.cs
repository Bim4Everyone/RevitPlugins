using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Services;
internal class GeometryService {
    /// <summary>
    /// Метод разделения зоны на полигоны
    /// </summary>    
    /// <remarks>
    /// В данном методе производится разделение зоны на полигоны (квадраты) и остаточные фигуры (не менее 3 сторон)
    /// </remarks>
    /// <returns>список Polygon</returns>
    public List<Polygon> DivideArea(Area area, double side, double minimalLength) {
        var opt = new SpatialElementBoundaryOptions();
        var loops = area.GetBoundarySegments(opt);
        if(loops == null || loops.Count == 0) {
            return [];
        }

        var segs = loops[0];
        var contour = segs.Select(s => s.GetCurve().GetEndPoint(0)).ToList();

        double minX = contour.Min(p => p.X);
        double maxX = contour.Max(p => p.X);
        double minY = contour.Min(p => p.Y);
        double maxY = contour.Max(p => p.Y);

        var polygons = new List<Polygon>();
        int created = 0;

        for(double x = minX; x <= maxX; x += side) {
            for(double y = minY; y <= maxY; y += side) {
                var p1 = new XYZ(x, y, 0);
                var p2 = new XYZ(x + side, y, 0);
                var p3 = new XYZ(x + side, y + side, 0);
                var p4 = new XYZ(x, y + side, 0);
                var square = new List<XYZ> { p1, p2, p3, p4 };

                var inside = square.Select(p => IsPointInsidePolygon(p, contour)).ToList();
                if(inside.All(b => !b)) {
                    continue; // полностью вне
                }

                var polyPoints = new List<XYZ>();
                // Добавим входящие углы
                for(int i = 0; i < 4; i++) {
                    if(inside[i]) {
                        polyPoints.Add(square[i]);
                    }
                }

                // Найти пересечения рёбер квадрата с рёбрами контура
                for(int i = 0; i < 4; i++) {
                    var segA = square[i];
                    var segB = square[(i + 1) % 4];
                    for(int j = 0; j < contour.Count; j++) {
                        var contourA = contour[j];
                        var contourB = contour[(j + 1) % contour.Count];
                        if(TryGetLinesIntersection(segA, segB, contourA, contourB, out var ip)) {
                            // Проверить, не дублируется ли точка
                            if(!polyPoints.Any(p => p.IsAlmostEqualTo(ip))) {
                                polyPoints.Add(ip);
                            }
                        }
                    }
                }

                // Если после отбора меньше 3 точек — не многоугольник, пропускаем
                if(polyPoints.Count < 3) {
                    continue;
                }
                // Сортируем точки полигона вокруг центра — чтобы построить верный контур
                var center = new XYZ(polyPoints.Average(p => p.X), polyPoints.Average(p => p.Y), 0);
                polyPoints = polyPoints
                    .OrderBy(pt => Math.Atan2(pt.Y - center.Y, pt.X - center.X))
                    .ToList();

                // Добавляем ребра
                var sideLines = new List<Line>();
                for(int i = 0; i < polyPoints.Count; i++) {
                    var from = polyPoints[i];
                    var to = polyPoints[(i + 1) % polyPoints.Count];
                    if(from.DistanceTo(to) > minimalLength) {
                        sideLines.Add(Line.CreateBound(from, to));
                    }
                    ;
                }
                // Добавьте этот многоугольник, если есть хотя бы 3 стороны
                if(sideLines.Count >= 3) {
                    polygons.Add(new Polygon {
                        Sides = sideLines,
                        Center = center
                    });
                }
                created++;
            }
        }
        return polygons;
    }

    /// <summary>
    /// Метод проверки точки нахождение точки внутри заданных точек
    /// </summary>    
    /// <remarks>
    /// В данном методе производится вычисление координат точки и сравнивание с заданными точками контура
    /// </remarks>
    /// <returns>bool</returns>   
    public bool IsPointInsidePolygon(XYZ p, List<XYZ> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            if((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y) &&
                p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) /
                 (polygon[j].Y - polygon[i].Y) + polygon[i].X) {
                inside = !inside;
            }
        }
        return inside;
    }

    public bool IsPointInsideFloorPolygon(XYZ p, List<List<XYZ>> outer, List<List<XYZ>> holes) {
        // 1. Должна быть внутри хотя бы одного внешнего контура
        bool insideOuter = false;
        foreach(var loop in outer) {
            if(IsPointInsidePolygon(p, loop)) {
                insideOuter = true;
                break;
            }
        }

        if(!insideOuter) {
            return false;
        }

        //2.Не должна быть внутри ни одной дыры
        foreach(var hole in holes) {
            if(IsPointInsidePolygon(p, hole)) {
                return false;
            }
        }

        return true;
    }


    public void ClassifyContours(List<List<XYZ>> contours, out List<List<XYZ>> outer, out List<List<XYZ>> holes) {
        outer = [];
        holes = [];

        int n = contours.Count;

        for(int i = 0; i < n; i++) {
            var loopI = contours[i];
            var testPoint = loopI[0]; // первая точка контура

            bool isHole = false;

            for(int j = 0; j < n; j++) {
                if(i == j) {
                    continue;
                }

                if(IsPointInsidePolygon(testPoint, contours[j])) {
                    // контур i находится внутри контура j
                    isHole = true;
                    break;
                }
            }

            if(isHole) {
                holes.Add(loopI);
            } else {
                outer.Add(loopI);
            }
        }
    }

    // Метод проверки, находятся ли линии ab и cd в единой плоскости и пересекаются ли
    private bool TryGetLinesIntersection(XYZ a, XYZ b, XYZ c, XYZ d, out XYZ intersection) {
        intersection = null;
        double ax = b.X - a.X;
        double ay = b.Y - a.Y;
        double bx = d.X - c.X;
        double by = d.Y - c.Y;

        double denominator = ax * by - ay * bx;
        if(Math.Abs(denominator) < 1e-10) {
            return false; // параллельно
        }

        double dx = c.X - a.X;
        double dy = c.Y - a.Y;

        double t = (dx * by - dy * bx) / denominator;
        double u = (dx * ay - dy * ax) / denominator;
        if(t < 0 || t > 1 || u < 0 || u > 1) {
            return false; // вне отрезков
        }

        intersection = new XYZ(a.X + t * ax, a.Y + t * ay, 0);
        return true;
    }

    //public IntersectionResult IsPointInsideSlabElement(XYZ point, SlabElement slabElement) {
    //    var upFaces = slabElement.Faces;
    //    foreach(var face in upFaces) {
    //        var result = face.Project(point);
    //        if(result != null) {
    //            return result;
    //        }
    //    }
    //    return null;
    //}

    //public XYZ IsPointInsideFloor(XYZ point, Floor floor) {
    //    var projectPoint = floor.GetVerticalProjectionPoint(point, FloorFace.Top);
    //    return projectPoint ?? null;
    //}
}
