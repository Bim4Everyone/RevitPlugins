using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabGeometryService(
    ISlabAnalyzeSlopeService slabAnalyzeSlopeService,
    ISlabFaceService slabFaceService)
    : ISlabGeometryService {

    // Метод получения геометрии плиты
    public SlabGeometryData GetSlabGeometryData(
        Floor floor,
        Transform transformFromDoc) {

        var slabType = slabAnalyzeSlopeService.GetSlabType(floor);

        // 1. Получаем реальные верхние поверхности
        // Они уже находятся в системе transformFromDoc.
        var realTopFaces = slabFaceService.GetRealTopFaces(
            floor,
            transformFromDoc);

        // 2. Получаем проверочные солиды
        // Они будут построены сразу в той же системе координат.
        var checkingSolids = CreateCheckingSolids(
            floor,
            realTopFaces,
            transformFromDoc);

        // 3. Убираем внутренние solids
        var validSolids = RemoveInnerSolids(checkingSolids);

        // 4. Контур берём из проверочных solids
        var contour = validSolids
            .Select(x => x.Loop)
            .ToList();

        // 5. Верхние грани зависят от типа перекрытия
        var topFaces = slabType is SlabType.Ruled or SlabType.SlopedPlanar
            ? realTopFaces
            : slabFaceService.GetFlatTopFaces(validSolids);

        return new SlabGeometryData {
            Contour = contour,
            TopFaces = topFaces,
            SlabType = slabType
        };
    }
    
    // Метод получения трансформации по старой и новой позициям
    public Transform CreateZTranslation(double oldPosition, double newPosition) {
        return Transform.CreateTranslation(new XYZ(0, 0, newPosition - oldPosition));
    }

    // Метод получения проверочных солидов плиты
    private List<(CurveLoop Loop, Solid Solid)> CreateCheckingSolids(
        Floor floor,
        List<Face> topFaces,
        Transform transformFromDoc) {

        var profile = GetProfile(floor);

        if (profile == null || profile.Size == 0) {
            return [];
        }

        // actualZ находится в той же системе координат,
        // что и topFaces.
        double actualZ = slabFaceService.GetMaxPointZ(topFaces);

        var result = new List<(CurveLoop Loop, Solid Solid)>();

        for (int i = 0; i < profile.Size; i++) {
            var curveArray = profile.get_Item(i);

            if (curveArray == null || curveArray.Size == 0) {
                continue;
            }

            var loop = TransformAndMoveLoopToZ(
                curveArray,
                transformFromDoc,
                actualZ);

            var solid = SolidUtility.ExtrudeSolid(
                [loop],
                up: false);

            if (solid == null || solid.Volume <= 0) {
                continue;
            }

            result.Add((loop, solid));
        }

        return result;
    }

    // Трансформирует Curve и после этого переносит её на targetZ.
    //
    // Тип исходной Curve сохраняется:
    // Line -> Line
    // Arc -> Arc
    // Ellipse -> Ellipse
    // NurbSpline -> NurbSpline
    // и т.д.
    private static CurveLoop TransformAndMoveLoopToZ(
        CurveArray curveArray,
        Transform transformFromDoc,
        double targetZ) {

        var transform = transformFromDoc ?? Transform.Identity;

        // Определяем Z первой кривой после основного transform.
        //
        // В нормальном Sketch все кривые находятся в одной плоскости,
        // поэтому достаточно взять Z первой точки.
        var firstCurve = curveArray
            .Cast<Curve>()
            .First();

        var transformedFirstCurve =
            firstCurve.CreateTransformed(transform);

        double currentZ =
            transformedFirstCurve.GetEndPoint(0).Z;

        double zOffset = targetZ - currentZ;

        var zTransform = Transform.CreateTranslation(
            new XYZ(0, 0, zOffset));

        var loop = new CurveLoop();

        foreach (Curve curve in curveArray) {
            var transformed = curve.CreateTransformed(transform);

            var moved = transformed.CreateTransformed(zTransform);

            loop.Append(moved);
        }

        return loop;
    }

    // Метод удаления внутренних солидов
    private static List<(CurveLoop Loop, Solid Solid)> RemoveInnerSolids(
        IReadOnlyList<(CurveLoop Loop, Solid Solid)> solids) {

        var removed = new HashSet<int>();

        for (int i = 0; i < solids.Count; i++) {
            for (int j = i + 1; j < solids.Count; j++) {

                if (!SolidUtility.IsIntersect(
                        solids[i].Solid,
                        solids[j].Solid)) {
                    continue;
                }

                double volumeI = solids[i].Solid.Volume;
                double volumeJ = solids[j].Solid.Volume;

                if (volumeI < volumeJ) {
                    removed.Add(i);
                }
                else if (volumeJ < volumeI) {
                    removed.Add(j);
                }
            }
        }

        return solids
            .Where((_, index) => !removed.Contains(index))
            .ToList();
    }

    // Метод получения профиля плиты
    private static CurveArrArray GetProfile(Floor floor) {
        var doc = floor.Document;
        var profileId = floor.SketchId;

        var sketch = doc.GetElement(profileId) as Sketch;

        return sketch?.Profile;
    }
}
