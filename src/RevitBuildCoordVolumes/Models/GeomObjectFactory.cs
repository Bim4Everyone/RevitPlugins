using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
    private readonly ISlabContourSolidService _slabContourSolidService;
    private readonly RevitRepository _revitRepository;

    public GeomObjectFactory(
        IContourService contourService, 
        IGeomObjectConnector geomObjectConnector, 
        IGeomObjectsSplitter geomObjectsSplitter,
        ISlabContourSolidService slabContourSolidService,
        RevitRepository revitRepository) {
        _contourService = contourService;
        _geomObjectConnector = geomObjectConnector;
        _geomObjectsSplitter = geomObjectsSplitter;
        _slabContourSolidService = slabContourSolidService;
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
            var firstRandomColumnObject = columnGroup.ColumnObjects[0];
            bool groupContainOneColumn = columnGroup.ColumnObjects.Count == 1;
            bool groupContainCurved = firstRandomColumnObject.StartSlab.SlabType == SlabType.Ruled || firstRandomColumnObject.FinishSlab.SlabType == SlabType.Ruled;
            bool groupContainSlope = firstRandomColumnObject.StartSlab.SlabType == SlabType.SlopedPlanar || firstRandomColumnObject.FinishSlab.SlabType == SlabType.SlopedPlanar;
            
            if(groupContainOneColumn || groupContainCurved || groupContainSlope) {
                var unitedObjects = CreateIndividualAndUnitedObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(unitedObjects);
            } else {
                var uniObjects = CreateUnitedContourGeomObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(uniObjects);
            }
        }
        return geomObjects;
    }
    
    public List<GeomObject> CreateSlabContourGeomObjects(IList<ColumnGroupObject> columnGroups, IList<PolygonObject> polygons, SpatialObject spatialObject, ProgressService progressService) {
        progressService?.BeginStage(ProgressType.BuildVolumes);
        int total = columnGroups.Count;
        int processed = 0;
        int reported = 0;
        var geomObjects = new List<GeomObject>();
        var spatialSolid = _slabContourSolidService.CreateSpatialSolid(spatialObject);
        
        foreach(var columnGroup in columnGroups) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();
            if(columnGroup.ColumnObjects.Count == 0) {
                continue;
            }
            
            var firstRandomColumnObject = columnGroup.ColumnObjects[0];
            bool groupContainOneColumn = columnGroup.ColumnObjects.Count == 1;
            bool groupContainCurved = firstRandomColumnObject.StartSlab.SlabType == SlabType.Ruled || firstRandomColumnObject.FinishSlab.SlabType == SlabType.Ruled;
            bool groupContainSlope = firstRandomColumnObject.StartSlab.SlabType == SlabType.SlopedPlanar || firstRandomColumnObject.FinishSlab.SlabType == SlabType.SlopedPlanar;
            
            if (groupContainOneColumn || groupContainCurved) {
                var unitedObjects = CreateIndividualAndUnitedObjects(columnGroup, polygons, progressService);
                geomObjects.AddRange(unitedObjects);
            } else if (groupContainSlope) {
                var slopedObjects = CreateSlopeSlabContourGeomObjects(columnGroup, spatialSolid, progressService);
                geomObjects.AddRange(slopedObjects);
            } else {
                var flatObjects = CreateFlatSlabContourGeomObjects(columnGroup, spatialSolid, progressService);
                geomObjects.AddRange(flatObjects);
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
        
        var firstColumnObject = columnGroups[0];
        
        var finalObjects = _geomObjectsSplitter.SplitGeomObjects(geomObjects, firstColumnObject, spatialObject, progressService);
        
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

    private List<GeomObject> CreateIndividualAndUnitedObjects(ColumnGroupObject columnGroup, IList<PolygonObject> polygons, ProgressService progressService) {
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

        var splitSolid = SolidUtils.SplitVolumes(solid);

        return [.. splitSolid
            .Select(solidPart => new GeomObject {
                GeometryObjects = [solidPart],
                LevelName = firstElement.LevelName,
                Volume = solidPart.Volume
            })];
    }

    private List<GeomObject> CreateIndividualColumnsGeomObjects(ColumnGroupObject columnGroupObject, IList<PolygonObject> polygons, ProgressService progressService) {
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

    private List<GeomObject> CreateFlatSlabContourGeomObjects(ColumnGroupObject columnGroupObject, Solid spatialSolid, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        var firstColumnObject = columns[0];
        
        var solid = _slabContourSolidService.CreateFlatSlabContourSolid(firstColumnObject, spatialSolid, progressService);
        
        if(solid is null) {
            return [];
        }

        return [new GeomObject {
            GeometryObjects = [solid],
            LevelName = firstColumnObject.LevelName,
            Volume = solid.Volume
        }];
    }

    private List<GeomObject> CreateSlopeSlabContourGeomObjects(ColumnGroupObject columnGroupObject, Solid spatialSolid, ProgressService progressService) {
        var columns = columnGroupObject.ColumnObjects;
        var firstElement = columns[0];

        var solids =
            _slabContourSolidService.CreateSlopedSlabContourSolids(firstElement, spatialSolid, progressService);

        if(solids.Count == 0) {
            return [];
        }

        return  [new GeomObject {
            GeometryObjects = solids.Cast<GeometryObject>().ToList(),
            LevelName = firstElement.LevelName,
            Volume = solids.Sum(s => s.Volume)
        }];
    }
}
