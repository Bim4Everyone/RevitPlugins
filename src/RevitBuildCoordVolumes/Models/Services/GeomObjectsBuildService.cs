using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Services;
internal class GeomObjectsBuildService : IGeomObjectsBuildService {
    private readonly IGeomObjectFactory _geomObjectFactory;
    private readonly IGeomObjectConnector _geomObjectConnector;

    public GeomObjectsBuildService(
        IGeomObjectFactory geomObjectFactory, 
        IGeomObjectConnector geomObjectConnector) {
        _geomObjectFactory = geomObjectFactory;
        _geomObjectConnector = geomObjectConnector;      
    }

    public List<GeomObject> CreateGeomObjects(
    BuildCoordVolumeSettings settings,
    IList<ColumnGroupObject> columnGroups,
    IList<PolygonObject> polygons,
    SpatialObject spatialObject,
    ProgressService progressService) {

        var geomObjects = new List<GeomObject>();
        switch (settings.BuilderMode) {
            case BuilderMode.ColumnBuilder: {
                var sepObjects = _geomObjectFactory.CreateIndividualColumnsGeomObjects(
                    columnGroups,
                    polygons,
                    progressService);

                geomObjects.AddRange(sepObjects);
                break;
            } case BuilderMode.ContourBuilder: {
                var sepObjects = _geomObjectFactory.CreateUnitedContourGeomObjects(
                    columnGroups,
                    polygons,
                    progressService);

                geomObjects.AddRange(sepObjects);
                break;
            } case BuilderMode.SlabBuilder or BuilderMode.AutomaticBuilder: {
                var sepObjects = _geomObjectFactory.CreateSlabContourGeomObjects(
                    columnGroups,
                    polygons,
                    spatialObject,
                    progressService);

                geomObjects.AddRange(sepObjects);
                break;
            }
        }

        // Финальное объединение.
        return settings.UnionVolumes 
            ? _geomObjectConnector.UnionGeomObjects(geomObjects, progressService) 
            : geomObjects;
    }
}
