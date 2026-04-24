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

    public GeomObjectsBuildService(IGeomObjectFactory geomObjectFactory, IGeomObjectConnector geomObjectConnector) {
        _geomObjectFactory = geomObjectFactory;
        _geomObjectConnector = geomObjectConnector;
    }

    public List<GeomObject> GetGeomObjects(
        BuildCoordVolumeSettings settings,
        IEnumerable<IGrouping<string, ColumnObject>> columnGroups,
        List<PolygonObject> polygons,
        ProgressService progressService) {

        // Получение режима работы билдера 
        var builderMode = settings.BuilderMode;

        // Получение списка GeomObject в зависимости от настроек        
        var geomObjects = new List<GeomObject>();
        foreach(var columnGroup in columnGroups) {
            var listColumns = columnGroup.ToList();
            var firstColumn = listColumns[0];
            bool alongObgect = listColumns.Count == 1;
            bool isSloped = firstColumn.IsSloped;

            if(builderMode == BuilderMode.ColumnBuilder) {
                var sepObjects = _geomObjectFactory.GetSeparatedGeomObjects(listColumns, polygons, progressService);
                geomObjects.AddRange(sepObjects);
            }

            if(builderMode is BuilderMode.ContourBuilder or BuilderMode.AutomaticBuilder) {
                if(alongObgect || isSloped) {
                    var sepObjects = _geomObjectFactory.GetSeparatedGeomObjects(listColumns, polygons, progressService);
                    var uniObjects = _geomObjectConnector.UnionGeomObjects(sepObjects, progressService);
                    geomObjects.AddRange(uniObjects);
                } else {
                    var uniObjects = _geomObjectFactory.GetUnitedGeomObjects(listColumns, polygons, progressService);
                    geomObjects.AddRange(uniObjects);
                }
            }
        }

        var finalObjects = builderMode == BuilderMode.AutomaticBuilder
            ? _geomObjectConnector.UnionGeomObjects(geomObjects, progressService)
            : geomObjects;

        return finalObjects;
    }
}
