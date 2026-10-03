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
    private readonly IGeomObjectsSplitter _geomObjectsSplitter;

    public GeomObjectsBuildService(
        IGeomObjectFactory geomObjectFactory, 
        IGeomObjectConnector geomObjectConnector, 
        IGeomObjectsSplitter geomObjectsSplitter) {
        _geomObjectFactory = geomObjectFactory;
        _geomObjectConnector = geomObjectConnector;
        _geomObjectsSplitter = geomObjectsSplitter;       
    }

    public List<GeomObject> GetGeomObjects(
    BuildCoordVolumeSettings settings,
    IEnumerable<IGrouping<string, ColumnObject>> columnGroups,
    List<PolygonObject> polygons,
    SpatialObject spatialObject,
    ProgressService progressService) {

        var builderMode = settings.BuilderMode;
        var listColumnGroups = columnGroups.ToList();

        var geomObjects = new List<GeomObject>();

        foreach(var columnGroup in listColumnGroups) {
            var listColumns = columnGroup.ToList();

            if(listColumns.Count == 0) {
                continue;
            }

            var firstColumn = listColumns[0];
            bool alongObject = listColumns.Count == 1;
            bool isSloped = firstColumn.IsSloped;

            if(builderMode is BuilderMode.ColumnBuilder) {
                var sepObjects = _geomObjectFactory.GetSeparatedGeomObjects(
                    listColumns,
                    polygons,
                    progressService);

                geomObjects.AddRange(sepObjects);
            }
            else if(builderMode is BuilderMode.ContourBuilder) {
                if(alongObject || isSloped) {
                    var sepObjects = _geomObjectFactory.GetSeparatedGeomObjects(
                        listColumns,
                        polygons,
                        progressService);

                    var uniObjects = _geomObjectConnector.UnionGeomObjects(
                        sepObjects,
                        progressService);

                    geomObjects.AddRange(uniObjects);
                } else {
                    var uniObjects = _geomObjectFactory.GetUnitedGeomObjects(
                        listColumns,
                        polygons,
                        progressService);

                    geomObjects.AddRange(uniObjects);
                }
            }
            else if(builderMode is BuilderMode.SlabBuilder or BuilderMode.AutomaticBuilder) {
                if(alongObject || isSloped) {
                    var sepObjects = _geomObjectFactory.GetSeparatedGeomObjects(
                        listColumns,
                        polygons,
                        progressService);

                    var uniObjects = _geomObjectConnector.UnionGeomObjects(
                        sepObjects,
                        progressService);

                    geomObjects.AddRange(uniObjects);
                } else {
                    var slabObjects = _geomObjectFactory.GetSlabContourGeomObjects(
                        listColumns,
                        progressService);

                    geomObjects.AddRange(slabObjects);
                }
            }
        }

        // Для Slab/Automatic сначала split.
        if(builderMode is BuilderMode.SlabBuilder or BuilderMode.AutomaticBuilder) {
            var listColumns = listColumnGroups[0].ToList();

            geomObjects = _geomObjectsSplitter.SplitGeomObjects(
                geomObjects,
                listColumns,
                spatialObject);
        }

        // Финальное объединение.
        if(settings.UnionVolumes) {
            geomObjects = _geomObjectConnector.UnionGeomObjects(
                geomObjects,
                progressService);
        }

        return geomObjects;
    }
}
