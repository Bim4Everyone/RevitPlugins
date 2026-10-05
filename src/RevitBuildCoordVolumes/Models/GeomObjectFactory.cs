using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
    private readonly RevitRepository _revitRepository;

    public GeomObjectFactory(
        IContourService contourService, 
        IGeomObjectConnector geomObjectConnector, 
        IGeomObjectsSplitter geomObjectsSplitter,
        RevitRepository revitRepository) {
        _contourService = contourService;
        _geomObjectConnector = geomObjectConnector;
        _geomObjectsSplitter = geomObjectsSplitter;
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
            bool groupContainSloped = firstRandomColumn.IsSloped;
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
        foreach(var columnGroup in columnGroups) {
            if(columnGroup.ColumnObjects.Count == 0) {
                continue;
            }
            bool groupContainOneColumn = columnGroup.ColumnObjects.Count == 1;
            var firstRandomColumn = columnGroup.ColumnObjects[0];
            bool groupContainSloped = firstRandomColumn.IsSloped;
            if(groupContainOneColumn || groupContainSloped) {
                var slopedObjects = CreateSlopedGeomObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(slopedObjects);
            }
            else {
                var uniObjects = CreateSlabContourGeomObjects(columnGroup, progressService);
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

    public List<GeomObject> CreateSlabContourGeomObjects(ColumnGroupObject columnGroupObject, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        var firstElement = columns[0];
        
        var firstSlab = firstElement.StartSlab;
        var lastSlab = firstElement.FinishSlab;
        
        var firstSolid = SolidUtility.ExtrudeSolid(firstSlab.TopContour, firstElement.StartPosition, firstElement.FinishPosition);
        var lastSolid = SolidUtility.ExtrudeSolid(lastSlab.TopContour, firstElement.StartPosition, firstElement.FinishPosition, false);
        
        if(firstSolid is null || lastSolid is null ) {
            return [];
        }

        var result = SolidUtility.IntersectSolid(firstSolid, lastSolid);
        
        if(result is null) {
            return [];
        }

        var geo = new GeomObject {
            GeometryObjects = [result],
            LevelName = firstElement.LevelName,
            Volume = result.Volume
        };

        return [geo];
    }
}
