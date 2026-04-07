using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class SlabBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly ISpatialElementDividerService _spatialElementDividerService;
    private readonly ISlabNormalizeService _slabNormalizeService;
    private readonly IColumnFactory _columnFactory;
    private readonly IGeomObjectFactory _geomObjectFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeServices _services;
    private readonly BuildCoordVolumeSettings _settings;

    public SlabBasedCoordVolumeBuilder(
        RevitRepository revitRepository,
        BuildCoordVolumeServices services,
        BuildCoordVolumeSettings settings) {
        _revitRepository = revitRepository;
        _services = services;
        _spatialElementDividerService = _services.SpatialDivider;
        _slabNormalizeService = _services.SlabNormalizer;
        _columnFactory = _services.ColumnFactory;
        _geomObjectFactory = _services.GeomObjectFactory;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject, ProgressService progressService) {
        // Разделение зоны на полигоны
        double squareSidePolygon = UnitUtils.ConvertToInternalUnits(_settings.SquareSideMm, UnitTypeId.Millimeters);
        double squareAngle = UnitUtils.ConvertToInternalUnits(_settings.SquareAngleDeg, UnitTypeId.Degrees);

        var polygons = _spatialElementDividerService.DivideSpatialElement(
            spatialObject.SpatialElement, squareSidePolygon, squareAngle, progressService);

        // Получение всех плит перекрытия из настроек
        var allSlabs = _revitRepository.GetSlabsByTypesDocsAndLevels(
            _settings.TypeSlabs, _settings.Documents, _settings.Levels).ToList();

        // Получение всех плит без отверстий (для плоских)
        var normalizedSlabs = GetNormalizeSlabs(allSlabs, progressService);

        // Построение групп колонн  
        var columnGroups = _columnFactory.GenerateColumnGroups(polygons, normalizedSlabs, progressService);

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
                    => _geomObjectFactory.GetSeparatedGeomObjects(listColumns, spatialElementPosition, progressService),
                BuilderMode.AutomaticBuilder
                    => _geomObjectFactory.GetUnitedGeomObjects(listColumns, spatialElementPosition, progressService),
                BuilderMode.ContourBuilder
                    => _geomObjectFactory.GetUnitedGeomObjects(listColumns, spatialElementPosition, progressService),
                BuilderMode.ColumnBuilder
                    => _geomObjectFactory.GetSeparatedGeomObjects(listColumns, spatialElementPosition, progressService),
                _ => []
            };
            geomObjects.AddRange(newObjects);
        }

        return geomObjects;
    }

    // Метод получения всех перекрытий без отверстий и вырезов (для плоских)
    private List<SlabElement> GetNormalizeSlabs(List<SlabElement> slabElements, ProgressService progressService
        ) {
        progressService?.BeginStage(ProgressType.SlabNormalize);
        int total = slabElements.Count;
        int processed = 0;
        int reported = 0;
        foreach(var slab in slabElements) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();
            bool isSloped = _slabNormalizeService.IsSloped(slab);
            var topFaces = _slabNormalizeService.GetTopFaces(slab);
            slab.IsSloped = isSloped;
            slab.TopFaces = slab.IsSloped
                ? topFaces
                : _slabNormalizeService.GetTopFacesClean(slab, topFaces);
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
        return slabElements;
    }
}
