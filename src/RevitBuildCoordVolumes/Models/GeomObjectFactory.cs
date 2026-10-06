using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;

internal class GeomObjectFactory : IGeomObjectFactory {
    private readonly IContourService _contourService;
    private readonly IGeomObjectConnector _geomObjectConnector;
    private readonly IGeomObjectsSplitter _geomObjectsSplitter;
    private readonly ISlabFaceService _slabFaceService;
    private readonly ISlabGeometryService _slabGeometryService;
    private readonly RevitRepository _revitRepository;

    public GeomObjectFactory(
        IContourService contourService, 
        IGeomObjectConnector geomObjectConnector, 
        IGeomObjectsSplitter geomObjectsSplitter,
        ISlabFaceService slabFaceService,
        ISlabGeometryService slabGeometryService,
        RevitRepository revitRepository) {
        _contourService = contourService;
        _geomObjectConnector = geomObjectConnector;
        _geomObjectsSplitter = geomObjectsSplitter;
        _slabFaceService = slabFaceService;
        _slabGeometryService = slabGeometryService;
        _revitRepository = revitRepository;
    }
    
    public List<GeomObject> CreateIndividualColumnsGeomObjects(IList<ColumnGroupObject> columnGroups, IList<PolygonObject> polygons, ProgressService progressService) {
        var geomObjects = new List<GeomObject>();
        foreach(var columnGroup in columnGroups) {
            var sepObjects = CreateIndividualColumnsGeomObjects(columnGroup, polygons, progressService);
            geomObjects.AddRange(sepObjects);
        }
        return geomObjects;
    }
    
    public List<GeomObject> CreateUnitedContourGeomObjects(IList<ColumnGroupObject> columnGroups, IList<PolygonObject> polygons, ProgressService progressService) {
        var geomObjects = new List<GeomObject>();
        foreach(var columnGroup in columnGroups) {
            if(columnGroup.ColumnObjects.Count == 0) {
                continue;
            }
            bool groupContainOneColumn = columnGroup.ColumnObjects.Count == 1;
            var firstRandomColumn = columnGroup.ColumnObjects[0];
            bool groupContainSloped = firstRandomColumn.StartSlab.SlabType != SlabType.Planar || firstRandomColumn.FinishSlab.SlabType != SlabType.Planar;
            if(groupContainOneColumn || groupContainSloped) {
                var slopedObjects = CreateSlopedGeomObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(slopedObjects);
            }
            else {
                var uniObjects = CreateUnitedContourGeomObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(uniObjects);
            }
        }
        return geomObjects;
    }
    
    public List<GeomObject> CreateSlabContourGeomObjects(IList<ColumnGroupObject> columnGroups, IList<PolygonObject> polygons, SpatialObject spatialObject, ProgressService progressService) {
        var geomObjects = new List<GeomObject>();
        var spatialSolid = ExtrudeSpatialObject(columnGroups, spatialObject);
        foreach(var columnGroup in columnGroups) {
            if(columnGroup.ColumnObjects.Count == 0) {
                continue;
            }
            bool groupContainOneColumn = columnGroup.ColumnObjects.Count == 1;
            var firstRandomColumn = columnGroup.ColumnObjects[0];
            bool groupContainCurved = firstRandomColumn.StartSlab.SlabType == SlabType.Ruled || firstRandomColumn.FinishSlab.SlabType == SlabType.Ruled;
            bool groupContainSlope = firstRandomColumn.StartSlab.SlabType == SlabType.SlopedPlanar || firstRandomColumn.FinishSlab.SlabType == SlabType.SlopedPlanar;
            
            if(groupContainOneColumn || groupContainCurved) {
                var slopedObjects = CreateSlopedGeomObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(slopedObjects);
            }
            
            if(groupContainSlope) {
                var slopedObjects = CreateSlopedSlabContourGeomObjects(columnGroup, spatialSolid, progressService);
                geomObjects.AddRange(slopedObjects);
            }
            
            else {
                var uniObjects = CreateSlabContourGeomObjects(columnGroup, spatialSolid, progressService);
                geomObjects.AddRange(uniObjects);
            }
        }
        var firstColumnObject = columnGroups[0];
        var finalObjects = _geomObjectsSplitter.SplitGeomObjects(
            geomObjects,
            firstColumnObject,
            spatialObject);
        
        return finalObjects;
    }
    
    public List<GeomObject> CreateSpatialExtrudeGeomObjects(BuildCoordVolumeSettings settings, SpatialObject spatialObject, ProgressService progressService) {
        progressService?.BeginStage(ProgressType.BuildVolumes);
        var spatialElement = spatialObject.SpatialElement;
        
        var topZoneParam = settings.ParamMaps
            .Where(param => param.Type == ParamType.TopZoneParam)
            .Select(param => param.SourceParam).First();

        var bottomZoneParam = settings.ParamMaps
            .Where(param => param.Type == ParamType.BottomZoneParam)
            .Select(param => param.SourceParam).First();

        double topPosition = _revitRepository.GetPositionInFeet(spatialElement, topZoneParam.Name);
        double bottomPosition = _revitRepository.GetPositionInFeet(spatialElement, bottomZoneParam.Name);

        double basePointOffset = _revitRepository.GetBasePointOffset();

        var listCurveLoops = _contourService.GetSimpleCurveLoops(spatialElement, bottomPosition, basePointOffset);
        var solid = SolidUtility.ExtrudeSolid(listCurveLoops, bottomPosition, topPosition);

        progressService?.ProgressCount?.Report(100);

        return solid == null
            ? []
            : [new GeomObject {
                GeometryObjects = [solid],
                Volume = solid.Volume
            }];
    }

    private List<GeomObject> CreateSlopedGeomObjects(ColumnGroupObject columnGroup, IList<PolygonObject> polygons, ProgressService progressService) {
        var sepObjects = CreateIndividualColumnsGeomObjects(columnGroup, polygons, progressService);
        return _geomObjectConnector.UnionGeomObjects(sepObjects, progressService);
    }

    private List<GeomObject> CreateUnitedContourGeomObjects(ColumnGroupObject columnGroupObject, IList<PolygonObject> polygons, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        double spatialElementPosition = polygons[0].Sides[0].GetEndPoint(0).Z;
        var firstElement = columns[0];
        double startExtrudePosition = firstElement.StartPosition;
        double finishExtrudePosition = firstElement.FinishPosition;

        var listCurveLoops = _contourService.GetColumnsCurveLoops(
            columns, spatialElementPosition, startExtrudePosition, progressService);

        var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);

        if(solid == null) {
            return [];
        }

        var splittedSolid = SolidUtils.SplitVolumes(solid);

        return [.. splittedSolid
            .Select(solid => new GeomObject {
                GeometryObjects = [solid],
                LevelName = firstElement.LevelName,
                Volume = solid.Volume
            })];
    }

    private List<GeomObject> CreateIndividualColumnsGeomObjects(
        ColumnGroupObject columnGroupObject, IList<PolygonObject> polygons, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        double spatialElementPosition = polygons[0].Sides[0].GetEndPoint(0).Z;
        var solids = new List<GeometryObject>();
        var volumes = new List<double>();
        var firstElement = columns[0];
        progressService?.BeginStage(ProgressType.BuildVolumes);
        int total = columns.Count;
        int processed = 0;
        int reported = 0;
        foreach(var column in columns) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();
            double startExtrudePosition = column.StartPosition;
            double finishExtrudePosition = column.FinishPosition;

            var listCurveLoops = _contourService.GetColumnCurveLoops(column, spatialElementPosition, startExtrudePosition);
            var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);
            if(solid != null) {
                solids.Add(solid);
                volumes.Add(solid.Volume);
            }
            processed++;
            int current = processed * 100 / total;
            if(current > 100) {
                current = 100;
            }
            if(current > reported) {
                reported = current;
                progressService?.ProgressCount?.Report(reported);
            }
        }

        return solids.Count == 0 || volumes.Count == 0
            ? []
            : [new GeomObject {
            GeometryObjects = solids,
            LevelName = firstElement.LevelName,
            Volume = volumes.Sum()
        }];
    }

    private List<GeomObject> CreateSlabContourGeomObjects(ColumnGroupObject columnGroupObject, Solid spatialSolid, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        var firstElement = columns[0];
        
        var startSlab = firstElement.StartSlab;
        var finishSlab = firstElement.FinishSlab;
        
        double minPointZ = _slabFaceService.GetMinPointZ(startSlab.TopFaces);
        double maxPointZ = _slabFaceService.GetMaxPointZ(finishSlab.TopFaces);
        
        var firstSolid = SolidUtility.ExtrudeSolid(startSlab.TopContour, minPointZ, maxPointZ);
        var lastSolid = SolidUtility.ExtrudeSolid(finishSlab.TopContour, minPointZ, maxPointZ, false);
        
        if(firstSolid is null || lastSolid is null ) {
            return [];
        }
        
        var result = SolidUtility.IntersectSolid(firstSolid, lastSolid);
        
        if(result is null || spatialSolid is null) {
            return [];
        }
        
        var finalResult = SolidUtility.IntersectSolid(result, spatialSolid);
        
        if(finalResult is null) {
            return [];
        }

        var geo = new GeomObject {
            GeometryObjects = [finalResult],
            LevelName = firstElement.LevelName,
            Volume = finalResult.Volume
        };
        return [geo];
    }
    
    private List<GeomObject> CreateSlopedSlabContourGeomObjects(ColumnGroupObject columnGroupObject, Solid spatialSolid, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        var firstElement = columns[0];
        
        var startSlab = firstElement.StartSlab;
        var finishSlab = firstElement.FinishSlab;
        
        double maxPointStartSlab = _slabFaceService.GetMaxPointZ(startSlab.TopFaces);
        double minPointZ = _slabFaceService.GetMinPointZ(startSlab.TopFaces);
        double maxPointZ = _slabFaceService.GetMaxPointZ(finishSlab.TopFaces);
        
        var startContour = startSlab.TopContour;
        var transform = _slabGeometryService.CreateZTranslation(maxPointStartSlab, minPointZ);
        foreach(var loop in startContour) {
            loop.Transform(transform);
        }
        
        var firstSolid = SolidUtility.ExtrudeSolid(startContour, minPointZ, maxPointZ);
        var lastSolid = SolidUtility.ExtrudeSolid(finishSlab.TopContour, minPointZ, maxPointZ, false);
        
        if(firstSolid is null || lastSolid is null ) {
            return [];
        }
        
        var result = SolidUtility.IntersectSolid(firstSolid, lastSolid);
        
        if(result is null || spatialSolid is null) {
            return [];
        }
        
        var finalResult = SolidUtility.IntersectSolid(result, spatialSolid);
        
        if(finalResult is null) {
            return [];
        }
        
        var startSlabSolid = startSlab.Floor.GetSolids().First();
        var finishSlabSolid = finishSlab.Floor.GetSolids().First();
        
        
        var cutSolids = new List<Solid> { finalResult };

        // ============================================================
        // Нижняя плита
        // ============================================================

        foreach (var face in startSlab.TopFaces) {
            var dividePlane = SolidUtility.GetPlaneFromFace(face);

            cutSolids = SplitSolids(cutSolids, dividePlane);
        }

        // Удаляем всё, что пересекает нижнюю плиту объемом
        cutSolids = cutSolids
            .Where(solid => !SolidUtility.IsIntersect(solid, startSlabSolid))
            .ToList();
        
        var uniSolid = SolidExtensions.CreateUnitedSolids(cutSolids).ToList();


        // ============================================================
        // Верхняя плита
        // ============================================================

        foreach (var face in finishSlab.TopFaces) {
            var dividePlane = SolidUtility.GetPlaneFromFace(face);

            uniSolid = SplitSolids(uniSolid, dividePlane);
        }

        // Оставляем только то, что пересекает верхнюю плиту объемом
        uniSolid = uniSolid
            .Where(solid => SolidUtility.IsIntersect(solid, finishSlabSolid))
            .ToList();
        
        uniSolid = SolidExtensions.CreateUnitedSolids(uniSolid).ToList();
        
        
        var geo = new GeomObject {
            GeometryObjects = uniSolid.Cast<GeometryObject>().ToList(),
            LevelName = firstElement.LevelName,
            Volume = 1
        };
        return [geo];
    }
    
    
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

    private Solid ExtrudeSpatialObject(IList<ColumnGroupObject> columnGroups, SpatialObject spatialObject) {
        var faces = new List<Face>();
        foreach(var columnGroupObject in columnGroups) {
            faces.AddRange(columnGroupObject.ColumnObjects.SelectMany(x => x.StartSlab.TopFaces));
            faces.AddRange(columnGroupObject.ColumnObjects.SelectMany(x => x.FinishSlab.TopFaces));
        }
        
        double minPointZ = _slabFaceService.GetMinPointZ(faces);
        double maxPointZ = _slabFaceService.GetMaxPointZ(faces);
        
        var contourCurves = _contourService.GetOuterContour(spatialObject.SpatialElement);
        var startContour = _contourService.GetCurveLoopsContour(contourCurves, null);
        double pointSpatial = startContour[0].ElementAt(0).GetEndPoint(0).Z;
        var transform = _slabGeometryService.CreateZTranslation(pointSpatial, minPointZ);
        foreach(var loop in startContour) {
            loop.Transform(transform);
        }
        
        return SolidUtility.ExtrudeSolid(startContour, minPointZ, maxPointZ);
    }
}
