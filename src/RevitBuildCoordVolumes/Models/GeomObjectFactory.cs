using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;

internal class GeomObjectFactory : IGeomObjectFactory {
    private readonly IContourService _contourService;

    public GeomObjectFactory(IContourService contourService) {
        _contourService = contourService;
    }

    public List<GeomObject> GetSimpleGeomObjects(
        SpatialElement spatialElement,
        double startExtrudePosition,
        double finishExtrudePosition,
        double basePointOffset,
        ProgressService progressService) {

        progressService?.BeginStage(ProgressType.BuildVolumes);

        var listCurveLoops = _contourService.GetSimpleCurveLoops(spatialElement, startExtrudePosition, basePointOffset);
        var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);

        progressService?.ProgressCount?.Report(100);

        return solid == null
            ? []
            : [new GeomObject {
                GeometryObjects = [solid],
                Volume = solid.Volume
            }];

    }

    public List<GeomObject> GetUnitedGeomObjects(
        List<ColumnObject> columns, double spatialElementPosition, ProgressService progressService) {
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
        List<ColumnObject> columns, double spatialElementPosition, ProgressService progressService) {
        var solids = new List<GeometryObject>();
        var volumes = new List<double>();
        var firstElement = columns[0];
        progressService?.BeginStage(ProgressType.BuildContour);
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
