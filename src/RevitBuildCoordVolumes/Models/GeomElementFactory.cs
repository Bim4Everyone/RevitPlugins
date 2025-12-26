using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models;

internal class GeomElementFactory : IGeomElementFactory {
    private readonly IContourService _contourService;

    public GeomElementFactory(IContourService contourService) {
        _contourService = contourService;
    }

    public GeomElement GetUnitedGeomElement(List<Column> columns, double spatialElementPosition) {
        var firstElement = columns[0];
        double startExtrudePosition = firstElement.StartPosition;
        double finishExtrudePosition = firstElement.FinishPosition;

        var listCurveLoops = _contourService.GetColumnsCurveLoops(columns, spatialElementPosition, startExtrudePosition);
        var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);

        return solid != null
            ? new GeomElement {
                GeometryObjects = [solid],
                FloorName = firstElement.LevelName
            }
            : null;
    }

    public GeomElement GetSeparatedGeomElement(List<Column> columns, double spatialElementPosition) {
        var solids = new List<GeometryObject>();
        var firstElement = columns[0];
        foreach(var column in columns) {
            double startExtrudePosition = column.StartPosition;
            double finishExtrudePosition = column.FinishPosition;

            var listCurveLoops = _contourService.GetColumnListCurveLoops(column, spatialElementPosition, startExtrudePosition);
            var solid = SolidUtility.ExtrudeSolid(listCurveLoops, startExtrudePosition, finishExtrudePosition);
            if(solid != null) {
                solids.Add(solid);
            }
        }
        return solids.Count != 0
            ? new GeomElement {
                GeometryObjects = solids,
                FloorName = firstElement.LevelName
            }
            : null;
    }
}
