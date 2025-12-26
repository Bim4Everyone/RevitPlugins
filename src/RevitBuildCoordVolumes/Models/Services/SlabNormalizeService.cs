using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SlabNormalizeService : ISlabNormalizeService {

    // Вычисляем верхние поверхности без отверстий и проемов
    public List<SlabElement> GetNormalizeSlabs(List<SlabElement> slabElements) {
        foreach(var slab in slabElements) {
            var upFaces = GetTopFaces(slab);
            bool isSloped = IsSloped(upFaces);
            slab.UpFaces = isSloped
                ? upFaces
                : GetTopFacesClean(slab, upFaces);
            slab.IsSloped = isSloped;
        }
        return slabElements;
    }

    private List<Face> GetTopFaces(SlabElement slabElement) {
        var floor = slabElement.Floor;
        return HostObjectUtils.GetTopFaces(floor)
            .Select(r => floor.GetGeometryObjectFromReference(r) as Face)
            .Where(f => f != null)
            .ToList();
    }

    private bool IsSloped(List<Face> faces) {
        const double tol = 1e-6;

        foreach(var face in faces) {
            // Нормаль берём как среднее значение нормали по UV домену
            var bbox = face.GetBoundingBox();
            double midU = (bbox.Min.U + bbox.Max.U) * 0.5;
            double midV = (bbox.Min.V + bbox.Max.V) * 0.5;

            var n = face.ComputeNormal(new UV(midU, midV));

            // Если Z-компонента нормали НЕ единица по модулю — грань не горизонтальная
            if(Math.Abs(Math.Abs(n.Z) - 1.0) > tol) {
                return true; // плитa наклонная или с измененной формой
            }
        }

        return false; // все грани горизонтальные
    }

    private List<Face> GetTopFacesClean(SlabElement slabElement, List<Face> upFaces) {
        var floor = slabElement.Floor;
        var transform = slabElement.Transform;

        try {
            var contour = FilterOuterContours(upFaces);
            var solid = SolidUtility.ExtrudeSolid(contour, 0, 1, false);

            if(solid == null) {
                return upFaces;
            }

            var transofrmedSolid = SolidUtils.CreateTransformed(solid, transform);

            var resultFaces = transofrmedSolid.Faces
                .Cast<Face>()
                .Where(f => {
                    var n = f.ComputeNormal(new UV(0.5, 0.5));
                    return n.Z > 0.95;
                })
                .ToList();

            return resultFaces;

        } catch {
            return upFaces;
        }
    }

    private List<CurveLoop> FilterOuterContours(List<Face> faces) {
        // ---- 1. Собираем все EdgeArray как отдельные контуры ----
        var contourEdges = new List<EdgeArray>();
        foreach(var face in faces) {
            foreach(EdgeArray loop in face.EdgeLoops) {
                contourEdges.Add(loop);
            }
        }

        // ---- 2. Определяем внешние контуры ----
        var outerCountours = new List<EdgeArray>();

        for(int i = 0; i < contourEdges.Count; i++) {
            var current = contourEdges[i];
            bool insideOther = false;

            // Любая точка любого ребра этого контура
            var samplePoint = current.get_Item(0).AsCurve().GetEndPoint(0);

            for(int j = 0; j < contourEdges.Count; j++) {
                if(i == j) {
                    continue;
                }

                // Проверяем: samplePoint этого контура внутри другого?
                if(IsPointInsideEdgeLoop(samplePoint, contourEdges[j])) {
                    insideOther = true;
                    break;
                }
            }

            if(!insideOther) {
                outerCountours.Add(current);
            }
        }

        // ---- 3. Превращаем оставшиеся EdgeArray → CurveLoop ----
        var loops = new List<CurveLoop>();

        foreach(var edgeArray in outerCountours) {
            // сортируем кривые в правильный контур
            var curves = new List<Curve>();
            foreach(Edge edge in edgeArray) {
                curves.Add(edge.AsCurve());
            }
            var orderedCurves = GeometryUtility.SortCurvesToLoop(curves);

            var curveLoop = new CurveLoop();
            foreach(var curve in orderedCurves) {
                curveLoop.Append(curve);
            }

            loops.Add(curveLoop);
        }

        return loops;
    }

    private bool IsPointInsideEdgeLoop(XYZ point3d, EdgeArray loop) {
        // Преобразуем все вершины кривых в список точек XY
        var poly = new List<XYZ>();

        foreach(Edge e in loop) {
            var curve = e.AsCurve();

            // Берём конец кривой - он гарантированно лежит в контуре
            poly.Add(new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, 0));
        }

        // Точку тоже проектируем в XY
        var p = new XYZ(point3d.X, point3d.Y, 0);

        return GeometryUtility.IsPointInsidePolygon(p, poly);
    }

}
