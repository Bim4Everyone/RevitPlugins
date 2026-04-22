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
    private readonly RevitRepository _revitRepository;

    public GeomObjectFactory(IContourService contourService, RevitRepository revitRepository) {
        _contourService = contourService;
        _revitRepository = revitRepository;
    }

    public List<GeomObject> GetSimpleGeomObjects(
        BuildCoordVolumeSettings settings,
        SpatialObject spatialObject,
        ProgressService progressService) {

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

    public List<GeomObject> GetUnitedGeomObjects(
        List<ColumnObject> columns, List<PolygonObject> polygons, ProgressService progressService) {
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
                FloorName = firstElement.FloorName,
                Volume = solid.Volume
            })];
    }

    public List<GeomObject> GetSeparatedGeomObjects(
        List<ColumnObject> columns, List<PolygonObject> polygons, ProgressService progressService) {
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
            FloorName = firstElement.FloorName,
            Volume = volumes.Sum()
        }];
    }
}
