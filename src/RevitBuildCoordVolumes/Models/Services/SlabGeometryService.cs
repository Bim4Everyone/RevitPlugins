using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

public class SlabGeometryService : ISlabGeometryService{
    
    private readonly ISlabAnalyzeSlopeService _slabAnalyzeSlopeService;
    private readonly IBoundingBoxService _boundingBoxService;

    public SlabGeometryService(ISlabAnalyzeSlopeService slabAnalyzeSlopeService, IBoundingBoxService boundingBoxService) {
        _slabAnalyzeSlopeService = slabAnalyzeSlopeService;
        _boundingBoxService = boundingBoxService;
    }
    
    public SlabGeometryData GetSlabGeometryData(Floor floor, Transform transformFromDoc) {
        bool isSloped = _slabAnalyzeSlopeService.IsSloped(floor);

        // 1. Строим проверочные solids из профиля.
        var checkingSolids = CreateCheckingSolids(floor, transformFromDoc);

        // 2. Убираем внутренние solids.
        var validSolids = RemoveInnerSolids(checkingSolids);

        // 3. Контур ВСЕГДА берём из проверочных solids.
        var contour = validSolids
            .Select(x => x.Loop)
            .ToList();

        // 4. Верхние грани зависят от типа перекрытия.
        var topFaces = isSloped
            ? GetSlopeTopFaces(floor, transformFromDoc)
            : GetFlatTopFaces(validSolids);

        return new SlabGeometryData {
            Contour = contour,
            TopFaces = topFaces,
            IsSloped = isSloped
        };
    }
    
    private List<(CurveLoop Loop, Solid Solid)> CreateCheckingSolids(Floor floor, Transform transformFromDoc) {
        var profile = GetProfile(floor);
        if (profile == null || profile.Size == 0) {
            return [];
        }
        double sketchZ = GetSketchZ(profile);
        double actualZ = GetMaxPointZ(floor, transformFromDoc);
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
    
    private static IList<Face> GetFlatTopFaces(IEnumerable<(CurveLoop Loop, Solid Solid)> solids) {
        return solids
            .SelectMany(x => x.Solid.Faces.Cast<Face>())
            .Where(face => IsFaceNormalWithinZRange(face, 0, 1))
            .ToList();
    }

    private IList<Face> GetSlopeTopFaces(Floor floor, Transform transformFromDoc) {
        var commonSolids = floor.GetSolids();
        var transformedSolids = commonSolids.Select(solid => SolidUtils.CreateTransformed(solid, transformFromDoc));
        return GetSolidsTopFaces(transformedSolids);
    }
    
    private IList<Face> GetSolidsTopFaces(IEnumerable<Solid> solids) {
        var faces =  solids
            .SelectMany(solid => solid.Faces
                .Cast<Face>()
                .Where(face => IsFaceNormalWithinZRange(face, 0, 1)))
            .ToList();
        return faces;
    }

    private double GetSketchZ(CurveArrArray curveArrArray) {
        return curveArrArray
            .Cast<CurveArray>()
            .Select(x => x
                .Cast<Curve>()
                .Select(y => y.GetEndPoint(0).Z)
                .Max())
            .Max();
    }
    
    // Метод получения трансформации по старой и новой позициям
    private Transform CreateZTranslation(double oldPosition, double newPosition) {
        return Transform.CreateTranslation(new XYZ(0, 0, newPosition - oldPosition));
    }
    
    private double GetMaxPointZ(Floor floor, Transform transformFromDoc) {
        var bbox = floor.get_BoundingBox(null);
        var transformBbox = _boundingBoxService.GetTransformedBoundingBox(bbox, transformFromDoc);
        return  transformBbox.Max.Z;
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
