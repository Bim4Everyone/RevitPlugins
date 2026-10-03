using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

public class SlabGeometryService(ISlabAnalyzeSlopeService slabAnalyzeSlopeService) : ISlabGeometryService {
    
    // Метод получения геометрии плиты
    public SlabGeometryData GetSlabGeometryData(Floor floor, Transform transformFromDoc) {
        bool isSloped = slabAnalyzeSlopeService.IsSloped(floor);
        
        // 1. Получаем реальные верхние поверхности
        var realTopFaces = GetRealTopFaces(floor, transformFromDoc);
        
        // 2. Получаем проверочные солиды
        var checkingSolids = CreateCheckingSolids(floor, realTopFaces);

        // 3. Убираем внутренние solids.
        var validSolids = RemoveInnerSolids(checkingSolids);

        // 4. Контур берём из проверочных solids.
        var contour = validSolids
            .Select(x => x.Loop)
            .ToList();

        // 5. Верхние грани зависят от типа перекрытия.
        var topFaces = isSloped
            ? realTopFaces
            : GetFlatTopFaces(validSolids);

        return new SlabGeometryData {
            Contour = contour,
            TopFaces = topFaces,
            IsSloped = isSloped
        };
    }
    
    // Метод получения реальных верхних граней плиты
    private List<Face> GetRealTopFaces(Floor floor, Transform transformFromDoc) {
        var commonSolids = floor.GetSolids();

        var transformedSolids = (transformFromDoc == null)
            ? commonSolids
            : commonSolids.Select(solid => SolidUtils.CreateTransformed(solid, transformFromDoc));
        
        return GetSolidsTopFaces(transformedSolids);
    }
    
    // Метод получения проверочных солидов плиты
    private List<(CurveLoop Loop, Solid Solid)> CreateCheckingSolids(Floor floor, List<Face> topFaces) {
        var profile = GetProfile(floor);
        if (profile == null || profile.Size == 0) {
            return [];
        }
        double sketchZ = GetSketchZ(profile);
        double actualZ = GetMaxPointZ(topFaces);
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
    
    // Метод получения реальных верхних граней плоской плиты
    private static List<Face> GetFlatTopFaces(IEnumerable<(CurveLoop Loop, Solid Solid)> solids) {
        return solids
            .SelectMany(x => x.Solid.Faces.Cast<Face>())
            .Where(face => IsFaceNormalWithinZRange(face, 0, 1))
            .ToList();
    }
    
    // Метод получения верхних граней из любых солидов
    private static List<Face> GetSolidsTopFaces(IEnumerable<Solid> solids) {
        var faces =  solids
            .SelectMany(solid => solid.Faces
                .Cast<Face>()
                .Where(face => IsFaceNormalWithinZRange(face, 0, 1)))
            .ToList();
        return faces;
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
    
    // Метод получения трансформации по старой и новой позициям
    private static Transform CreateZTranslation(double oldPosition, double newPosition) {
        return Transform.CreateTranslation(new XYZ(0, 0, newPosition - oldPosition));
    }
    
    // Метод получения самой верхней точки Face
    private double GetMaxPointZ(List<Face> topFaces) {
        double maxZ = double.MinValue;
        foreach (var face in topFaces) {
            foreach (EdgeArray edgeLoop in face.EdgeLoops) {
                foreach (Edge edge in edgeLoop) {
                    var curve = edge.AsCurve();
                    
                    var p0 = curve.GetEndPoint(0);
                    var p1 = curve.GetEndPoint(1);

                    maxZ = Math.Max(maxZ, p0.Z);
                    maxZ = Math.Max(maxZ, p1.Z);
                }
            }
        }
        return maxZ;
    }
    
    // Метод определения, входит ли значение нормали Face в заданный диапазон
    private static bool IsFaceNormalWithinZRange(Face face, double minValue, double maxValue) {
        double normalZ = face.ComputeNormal(new UV(0.5, 0.5)).Normalize().Z;
        return normalZ > minValue && normalZ <= maxValue;
    }
    
    // Метод получения профиля плиты
    private static CurveArrArray GetProfile(Floor floor) {
        var doc = floor.Document;
        var profileId = floor.SketchId;
        var sketch = doc.GetElement(profileId) as Sketch;
        return sketch?.Profile;
    }
}
