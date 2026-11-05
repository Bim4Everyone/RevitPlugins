using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabContourSolidService : ISlabContourSolidService {
    public Solid CreateFlatSlabIntersectionSolid(ColumnObject columnObject, Solid spatialSolid) {
        
        if(spatialSolid is null) {
            return null;
        }
        
        var firstSolid = SolidUtility.ExtrudeSolid(columnObject.StartSlab.TopContour, columnObject.StartPosition, columnObject.FinishPosition);
        
        if(firstSolid is null) {
            return null;
        }
        
        
        var lastSolid = SolidUtility.ExtrudeSolid(columnObject.FinishSlab.TopContour, columnObject.StartPosition, columnObject.FinishPosition, false);
        
        if(lastSolid is null ) {
            return null;
        }
        
        SolidUtility.TryGetBooleanSolid(firstSolid, lastSolid, BooleanOperationsType.Intersect, out var result);
        
        return result is null 
            ? null 
            : IntersectBySpatialSolid(result, spatialSolid);
    }
    
     public IList<Solid> CreateSlopedSlabIntersectionSolids(ColumnObject columnObject, Solid spatialSolid) {
        
         if(spatialSolid is null) {
             return [];
         }
         
        var startSlab = columnObject.StartSlab;
        var finishSlab = columnObject.FinishSlab;
        
        double minPointZ = startSlab.MinPointZ;
        double maxPointZ = finishSlab.MaxPointZ;
        
        var startContour = startSlab.BottomContour;
        
        var firstSolid = SolidUtility.ExtrudeSolid(startContour, minPointZ, maxPointZ);
        
        if(firstSolid is null) {
            return [];
        }
        
        var lastSolid = SolidUtility.ExtrudeSolid(finishSlab.TopContour, minPointZ, maxPointZ, false);
        
        if(lastSolid is null ) {
            return [];
        }

        SolidUtility.TryGetBooleanSolid(firstSolid, lastSolid, BooleanOperationsType.Intersect, out var result);
        
        if(result is null) {
            return [];
        }
        
        var finalResult = IntersectBySpatialSolid(result, spatialSolid);
        
        if(finalResult is null) {
            return [];
        }
        
        var startSlabSolids = startSlab.Floor.GetSolids().ToList();
        var finishSlabSolids = finishSlab.Floor.GetSolids().ToList();

        if(startSlabSolids.Count == 0 || finishSlabSolids.Count == 0) {
            return [];
        }

        var cutSolids = new List<Solid> { finalResult };

        // ============================================================
        // Нижняя плита
        // ============================================================

        foreach (var face in startSlab.TopFaces) {
            var dividePlane = SolidUtility.GetPlaneFromFace(face);
            cutSolids = SplitSolids(cutSolids, dividePlane);
        }

        // Удаляем всё, что пересекается с любым солидом нижней плиты
        cutSolids = cutSolids
            .Where(solid => !startSlabSolids.Any(slabSolid =>
                SolidUtility.IsIntersect(solid, slabSolid)))
            .ToList();

        if (cutSolids.Count == 0) {
            return [];
        }
        
        var uniSolid = SolidExtensions.CreateUnitedSolids(cutSolids).ToList();


        // ============================================================
        // Верхняя плита
        // ============================================================

        foreach (var face in finishSlab.TopFaces) {
            var dividePlane = SolidUtility.GetPlaneFromFace(face);
            uniSolid = SplitSolids(uniSolid, dividePlane);
        }

        // Оставляем только то, что пересекается хотя бы с одним солидом верхней плиты
        uniSolid = uniSolid
            .Where(solid => finishSlabSolids.Any(slabSolid =>
                SolidUtility.IsIntersect(solid, slabSolid)))
            .ToList();
        
        if (uniSolid.Count == 0) {
            return [];
        }
        
        return SolidExtensions.CreateUnitedSolids(uniSolid).ToList();
    }

    public IList<SolidObject> CreateBottomSlabExtrudeSolids(IList<SlabElement> filteredSlabs, Solid spatialSolid) {
        var sortedSlabs = filteredSlabs
            .OrderBy(x => x.MinPointZ)
            .ToList();
        var result = new List<SolidObject>();
        for(int i = 0; i < sortedSlabs.Count - 1; i++) {
            var bottomSlab = sortedSlabs[i];
            var topSlab = sortedSlabs[i + 1];

            var solid = SolidUtility.ExtrudeSolid(bottomSlab.BottomContour, bottomSlab.MinPointZ, topSlab.MinPointZ);

            if(solid is null) {
                continue;
            }
            var finalResult = IntersectBySpatialSolid(solid, spatialSolid);
            
            if(finalResult is null) {
                continue;
            }
            result.Add(new SolidObject {Solid = finalResult, LevelName = bottomSlab.LevelName});
        }
        return result;
    }

    public IList<SolidObject> CreateTopSlabExtrudeSolids(IList<SlabElement> filteredSlabs, Solid spatialSolid) {
        var sortedSlabs = filteredSlabs
            .OrderByDescending(x => x.MinPointZ)
            .ToList();
        
        var result = new List<SolidObject>();
        for(int i = 0; i < sortedSlabs.Count - 1; i++) {
            var topSlab = sortedSlabs[i];
            var bottomSlab  = sortedSlabs[i + 1];

            var solid = SolidUtility.ExtrudeSolid(topSlab.TopContour, bottomSlab.MinPointZ, topSlab.MinPointZ, false);

            if(solid is null) {
                continue;
            }
            var finalResult = IntersectBySpatialSolid(solid, spatialSolid);
            
            if(finalResult is null) {
                continue;
            }
            result.Add(new SolidObject {Solid = finalResult, LevelName = topSlab.LevelName});
        }
        return result;
    }

    // Метод, в котором список солидов разделяется секущими плоскостями
    private static List<Solid> SplitSolids(List<Solid> solids, DividePlane dividePlane) {
        var result = new List<Solid>();

        foreach (var solid in solids) {
            var negativeSolid = SolidUtility.DivideSolidSafe(solid, dividePlane.NegativePlane);

            var positiveSolid = SolidUtility.DivideSolidSafe(solid, dividePlane.PositivePlane);

            if (negativeSolid != null)
                result.Add(negativeSolid);

            if (positiveSolid != null)
                result.Add(positiveSolid);
        }

        return result;
    }

    private static Solid IntersectBySpatialSolid(Solid solid, Solid spatialSolid) {
        if(solid is null) {
            return null;
        }
        SolidUtility.TryGetBooleanSolid(solid, spatialSolid, BooleanOperationsType.Intersect, out var finalResult);
        return finalResult;
    }
}
