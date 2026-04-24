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

        // Получение типа построения
        var buildType = settings.BuildType;

        // Получение списка GeomObject в зависимости от настроек        
        var geomObjects = new List<GeomObject>();
        foreach(var columnGroup in columnGroups) {
            var listColumns = columnGroup.ToList();
            var firstColumn = listColumns[0];

            bool useSeparated =
                builderMode == BuilderMode.ColumnBuilder ||
                (builderMode == BuilderMode.AutomaticBuilder &&
                (firstColumn.IsSloped || listColumns.Count == 1));

            var newObjects = useSeparated
                ? _geomObjectFactory.GetSeparatedGeomObjects(listColumns, polygons, progressService)
                : _geomObjectFactory.GetUnitedGeomObjects(listColumns, polygons, progressService);

            geomObjects.AddRange(newObjects);
        }

        bool useConnector = buildType == BuildType.Union;

        var modifyObjects = useConnector
            ? _geomObjectConnector.UnionGeomObjects(geomObjects)
            : geomObjects;

        return modifyObjects;
    }
}
