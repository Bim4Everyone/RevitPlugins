using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class SlabBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly ISpatialElementDividerService _spatialElementDividerService;
    private readonly ISlabNormalizeService _slabNormalizeService;
    private readonly IColumnFactory _columnFactory;
    private readonly IGeomObjectFactory _geomObjectFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;

    public SlabBasedCoordVolumeBuilder(
        ISpatialElementDividerService spatialElementDividerService,
        ISlabNormalizeService slabNormalizeService,
        IColumnFactory columnFactory,
        IGeomObjectFactory geomObjectFactory,
        RevitRepository revitRepository,
        BuildCoordVolumeSettings settings) {
        _spatialElementDividerService = spatialElementDividerService;
        _slabNormalizeService = slabNormalizeService;
        _columnFactory = columnFactory;
        _geomObjectFactory = geomObjectFactory;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject) {
        // Разделение зоны на полигоны
        double squareSidePolygon = UnitUtils.ConvertToInternalUnits(_settings.SquareSideMm, UnitTypeId.Millimeters);
        double squareAngle = UnitUtils.ConvertToInternalUnits(_settings.SquareAngleDeg, UnitTypeId.Degrees);
        var polygons = _spatialElementDividerService.DivideSpatialElement(spatialObject.SpatialElement, squareSidePolygon, squareAngle);

        // Получение всех плит перекрытия из настроек
        var allSlabs = _revitRepository.GetSlabsByTypesDocsAndLevels(
            _settings.TypeSlabs, _settings.Documents, _settings.Levels).ToList();

        // Получение всех плит без отверстий (для плоских)
        var normalizedSlabs = GetNormalizeSlabs(allSlabs);

        // Построение групп колонн  
        var columnGroups = _columnFactory.GenerateColumnGroups(polygons, normalizedSlabs);

        // Получение ориентации полигона для Transform
        double spatialElementPosition = polygons[0].Sides[0].GetEndPoint(0).Z;

        // Получение режима работы билдера 
        var builderMode = _settings.BuilderMode;

        // Получение списка GeomObject в зависимости от настроек
        var geomObjects = new List<GeomObject>();
        foreach(var columnGroup in columnGroups) {
            var listColumns = columnGroup.ToList();
            var firstColumn = listColumns[0];

            var newObjects = builderMode switch {
                BuilderMode.AutomaticBuilder when firstColumn.IsSloped || listColumns.Count == 1
                    => _geomObjectFactory.GetSeparatedGeomObjects(listColumns, spatialElementPosition),
                BuilderMode.AutomaticBuilder
                    => _geomObjectFactory.GetUnitedGeomObjects(listColumns, spatialElementPosition),
                BuilderMode.ContourBuilder
                    => _geomObjectFactory.GetUnitedGeomObjects(listColumns, spatialElementPosition),
                BuilderMode.ColumnBuilder
                    => _geomObjectFactory.GetSeparatedGeomObjects(listColumns, spatialElementPosition),
                _ => []
            };

            geomObjects.AddRange(newObjects);
        }

        return geomObjects;
    }

    // Метод получения всех перекрытий без отверстий и вырезов (для плоских)
    private List<SlabElement> GetNormalizeSlabs(List<SlabElement> slabElements) {
        foreach(var slab in slabElements) {
            var topFaces = _slabNormalizeService.GetTopFaces(slab);
            slab.TopFaces = slab.IsSloped
                ? topFaces
                : _slabNormalizeService.GetTopFacesClean(slab, topFaces);
        }
        return slabElements;
    }
}
