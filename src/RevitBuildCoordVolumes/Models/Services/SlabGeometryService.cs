using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

public class SlabGeometryService(ISlabAnalyzeSlopeService slabAnalyzeSlopeService, ISlabFaceService slabFaceService) : ISlabGeometryService {
    
    // Метод получения геометрии плиты
    public SlabGeometryData GetSlabGeometryData(Floor floor, Transform transformFromDoc) {
        var slabType = slabAnalyzeSlopeService.GetSlabType(floor);
        
        // 1. Получаем реальные верхние поверхности
        var realTopFaces = slabFaceService.GetRealTopFaces(floor, transformFromDoc);
        
        // 2. Получаем проверочные солиды
        var checkingSolids = CreateCheckingSolids(floor, realTopFaces);

        // 3. Убираем внутренние solids.
        var validSolids = RemoveInnerSolids(checkingSolids);

        // 4. Контур берём из проверочных solids.
        var contour = validSolids
            .Select(x => x.Loop)
            .ToList();

        // 5. Верхние грани зависят от типа перекрытия.
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
    private List<(CurveLoop Loop, Solid Solid)> CreateCheckingSolids(Floor floor, List<Face> topFaces) {
        var profile = GetProfile(floor);
        if (profile == null || profile.Size == 0) {
            return [];
        }
        double sketchZ = GetSketchZ(profile);
        double actualZ = slabFaceService.GetMaxPointZ(topFaces);
        var transform = CreateZTranslation(sketchZ, actualZ);
        var result = new List<(CurveLoop Loop, Solid Solid)>();

        for (int i = 0; i < profile.Size; i++) {
            var curveArray = profile.get_Item(i);
            var curves = curveArray
                .Cast<Curve>()
                .ToList();

            if (curves.Count == 0) {
                continue;
            }

            var loop = CurveLoop.Create(curves);
            loop.Transform(transform);
            
            var solid = SolidUtility.ExtrudeSolid([loop], up: false);

            result.Add((loop, solid));
        }
        return result;
    }
    
    // Метод удаления внутренних солидов (которые находятся внутри других солидов)
    private static List<(CurveLoop Loop, Solid Solid)> RemoveInnerSolids(IReadOnlyList<(CurveLoop Loop, Solid Solid)> solids) {
        var result = new List<(CurveLoop Loop, Solid Solid)>();

        for (int i = 0; i < solids.Count; i++) {
            var current = solids[i];
            bool intersectsAnotherSolid = false;
            for (int j = 0; j < solids.Count; j++) {
                if (i == j) {
                    continue;
                }

                if(!SolidUtility.IsIntersect(current.Solid, solids[j].Solid)) {
                    continue;
                }
                intersectsAnotherSolid = true;
                break;
            }

            if (!intersectsAnotherSolid) {
                result.Add(current);
            }
        }
        return result;
    }

    // Метод получения Z координаты плоской поверхности
    private static double GetSketchZ(CurveArrArray curveArrArray) {
        return curveArrArray
            .Cast<CurveArray>()
            .Select(x => x
                .Cast<Curve>()
                .Select(y => y.GetEndPoint(0).Z)
                .Max())
            .Max();
    }
    
    // Метод получения профиля плиты
    private static CurveArrArray GetProfile(Floor floor) {
        var doc = floor.Document;
        var profileId = floor.SketchId;
        var sketch = doc.GetElement(profileId) as Sketch;
        return sketch?.Profile;
    }
}
