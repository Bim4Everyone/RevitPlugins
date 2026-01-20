using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;
internal class ContourService : IContourService {

    public List<CurveLoop> GetColumnsCurveLoops(List<ColumnObject> columns, double spatialElementPosition, double startExtrudePosition) {
        // Получаем все линии полигонов
        var allLines = columns
            .SelectMany(column => column.PolygonObject.Sides)
            .ToList();

        // Удаляем дубликаты
        var uniqueLines = GetUniqueLines(allLines)
            .Cast<Curve>()
            .ToList();

        // Получаем актуальную трансформацию
        var transform = GetTransform(spatialElementPosition, startExtrudePosition);

        return GetCurveLoopsContour(uniqueLines, transform);
    }

    public List<CurveLoop> GetColumnCurveLoops(ColumnObject column, double spatialElementPosition, double startExtrudePosition) {
        // Получаем все линии полигонов
        var allLines = column.PolygonObject.Sides
            .Cast<Curve>()
            .ToList();

        // Получаем актуальную трансформацию
        var transform = GetTransform(spatialElementPosition, startExtrudePosition);

        return GetCurveLoopsContour(allLines, transform);
    }

    public List<CurveLoop> GetSimpleCurveLoops(SpatialElement spatialElement, double startExtrudePosition) {
        var opt = new SpatialElementBoundaryOptions();
        var loops = spatialElement.GetBoundarySegments(opt);
        if(loops == null || loops.Count == 0) {
            return [];
        }

        var segs = loops[0];
        var contour = segs.Select(s => s.GetCurve()).ToList();

        // Получаем ориентацию полигона для Transform
        double spatialElementPosition = contour[0].GetEndPoint(0).Z;

        // Получаем актуальную трансформацию
        var transform = GetTransform(spatialElementPosition, startExtrudePosition);

        return GetCurveLoopsContour(contour, transform);
    }

    private List<CurveLoop> GetCurveLoopsContour(List<Curve> allCurves, Transform transform) {
        var devidedCurves = SplitToLoopsOptimized(allCurves);
        var curveLoops = new List<CurveLoop>();
        foreach(var listCurve in devidedCurves) {
            var curveLoop = new CurveLoop();
            foreach(var curve in listCurve) {
                curveLoop.Append(curve);
            }
            if(!curveLoop.IsOpen()) {
                curveLoop.Transform(transform);
                curveLoops.Add(curveLoop);
            }
        }
        return curveLoops;
    }

    private Transform GetTransform(double oldPosition, double newPosition) {
        return Transform.CreateTranslation(new XYZ(0, 0, newPosition - oldPosition));
    }

    private List<Line> GetUniqueLines(List<Line> lines) {
        var map = new Dictionary<string, Line>(lines.Count);

        static string GetKey(Line line) {
            // сортируем концы по координатам
            double x1 = line.GetEndPoint(0).X, y1 = line.GetEndPoint(0).Y;
            double x2 = line.GetEndPoint(1).X, y2 = line.GetEndPoint(1).Y;

            if(x1 > x2 || (x1 == x2 && y1 > y2)) {
                (x1, y1, x2, y2) = (x2, y2, x1, y1);
            }

            // округление координат для стабильности
            const double tol = 1e-6;
            long qx1 = (long) Math.Round(x1 / tol);
            long qy1 = (long) Math.Round(y1 / tol);
            long qx2 = (long) Math.Round(x2 / tol);
            long qy2 = (long) Math.Round(y2 / tol);

            return $"{qx1}_{qy1}_{qx2}_{qy2}";
        }

        foreach(var line in lines) {
            string key = GetKey(line);

            if(map.TryGetValue(key, out var existing)) {
                if(line.Intersect(existing) == SetComparisonResult.Equal) {
                    // удаляем обе линии
                    map.Remove(key);
                }
            } else {
                map.Add(key, line);
            }
        }

        return map.Values.ToList();
    }

    private List<List<Curve>> SplitToLoopsOptimized(List<Curve> allCurves) {
        var remaining = new HashSet<Curve>(allCurves);
        var pointMap = new Dictionary<string, List<Curve>>();

        const double tol = 1e-6;

        static string GetPointKey(XYZ pt) {
            long x = (long) Math.Round(pt.X / tol);
            long y = (long) Math.Round(pt.Y / tol);
            long z = (long) Math.Round(pt.Z / tol);
            return $"{x}_{y}_{z}";
        }

        // строим карту точек
        foreach(var curve in allCurves) {
            foreach(var pt in new[] { curve.GetEndPoint(0), curve.GetEndPoint(1) }) {
                string key = GetPointKey(pt);
                if(!pointMap.TryGetValue(key, out var list)) {
                    list = [];
                    pointMap[key] = list;
                }
                list.Add(curve);
            }
        }

        var loops = new List<List<Curve>>();

        while(remaining.Count > 0) {
            var start = remaining.First();
            remaining.Remove(start);

            var loop = new List<Curve> { start };
            var queue = new Queue<Curve>();
            queue.Enqueue(start);

            while(queue.Count > 0) {
                var current = queue.Dequeue();
                foreach(var pt in new[] { current.GetEndPoint(0), current.GetEndPoint(1) }) {
                    string key = GetPointKey(pt);
                    if(pointMap.TryGetValue(key, out var candidates)) {
                        foreach(var c in candidates.ToList()) {
                            if(remaining.Contains(c)) {
                                remaining.Remove(c);
                                loop.Add(c);
                                queue.Enqueue(c);
                            }
                        }
                    }
                }
            }

            // сортируем в непрерывный контур
            var sortedLoop = GeometryUtility.SortCurvesToLoop(loop);
            loops.Add(sortedLoop);
        }

        return loops;
    }
}
