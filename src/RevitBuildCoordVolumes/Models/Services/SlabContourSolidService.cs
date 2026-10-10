using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabContourSolidService : ISlabContourSolidService {
    private readonly IContourService _contourService;
    private readonly ISlabFaceService _slabFaceService;

    public SlabContourSolidService(IContourService contourService, ISlabFaceService slabFaceService) {
        _contourService = contourService; 
        _slabFaceService = slabFaceService;
    }
    
    public Solid CreateSpatialSolid(SpatialObject spatialObject) {
        var contourCurves = _contourService.GetOuterContour(spatialObject.SpatialElement);
        var startContour = _contourService.GetCurveLoopsContour(contourCurves, null);
        double pointSpatial = startContour[0].ElementAt(0).GetEndPoint(0).Z;
        var transform = GeometryUtility.CreateZTranslation(pointSpatial, -1000);
        foreach(var loop in startContour) {
            loop.Transform(transform);
        }
        return SolidUtility.ExtrudeSolid(startContour, -1000, 1000);
    }
    
    public Solid CreateFlatSlabContourSolid(ColumnObject columnObject, Solid spatialSolid, ProgressService progressService) {
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
        
        if(result is null) {
            return null;
        }
        
        SolidUtility.TryGetBooleanSolid(result, spatialSolid, BooleanOperationsType.Intersect, out var finalResult);
        
        return finalResult;
    }
    
     public IList<Solid> CreateSlopedSlabContourSolids(ColumnObject columnObject, Solid spatialSolid, ProgressService progressService) {
         if(spatialSolid is null) {
             return [];
         }
        var startSlab = columnObject.StartSlab;
        var finishSlab = columnObject.FinishSlab;
        
        double minPointZ = _slabFaceService.GetMinPointZ(startSlab.TopFaces);
        double maxPointZ = _slabFaceService.GetMaxPointZ(finishSlab.TopFaces);
        
        var startContour = GetStartSlabContour(startSlab, minPointZ);
        
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
        
        SolidUtility.TryGetBooleanSolid(result, spatialSolid, BooleanOperationsType.Intersect, out var finalResult);
        
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
    
    private List<CurveLoop> GetStartSlabContour(SlabElement startSlab, double minPointZ) {
        if(startSlab.SlabType == SlabType.Planar) {
            return startSlab.TopContour;
        }

        double maxPointStartSlab = _slabFaceService.GetMaxPointZ(startSlab.TopFaces);
        var startContour = startSlab.TopContour;
        var transform = GeometryUtility.CreateZTranslation(maxPointStartSlab, minPointZ);
        foreach(var loop in startContour) {
            loop.Transform(transform);
        }
        return startContour;
    }
    
    
}
