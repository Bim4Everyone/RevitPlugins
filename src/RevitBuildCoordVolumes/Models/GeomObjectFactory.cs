using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
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
        double basePointOffset) {

        var listCurveLoops = _contourService.GetSimpleCurveLoops(spatialElement, startExtrudePosition, basePointOffset);
        var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);

        return solid == null
            ? []
            : [new GeomObject {
                GeometryObjects = [solid],
                Volume = solid.Volume
            }];
    }

    public List<GeomObject> GetUnitedGeomObjects(List<ColumnObject> columns, double spatialElementPosition) {
        var firstElement = columns[0];
        double startExtrudePosition = firstElement.StartPosition;
        double finishExtrudePosition = firstElement.FinishPosition;

        var listCurveLoops = _contourService.GetColumnsCurveLoops(columns, spatialElementPosition, startExtrudePosition);
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

    public List<GeomObject> GetSeparatedGeomObjects(List<ColumnObject> columns, double spatialElementPosition) {
        var solids = new List<GeometryObject>();
        var volumes = new List<double>();
        var firstElement = columns[0];
        foreach(var column in columns) {
            double startExtrudePosition = column.StartPosition;
            double finishExtrudePosition = column.FinishPosition;

            var listCurveLoops = _contourService.GetColumnCurveLoops(column, spatialElementPosition, startExtrudePosition);
            var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);
            if(solid != null) {
                solids.Add(solid);
                volumes.Add(solid.Volume);
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
