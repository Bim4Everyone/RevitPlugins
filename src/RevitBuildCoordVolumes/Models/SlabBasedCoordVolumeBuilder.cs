using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class SlabBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly ISpatialElementDividerService _spatialElementDividerService;
    private readonly IColumnFactory _columnFactory;
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
        _columnFactory = _services.ColumnFactory;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject, ProgressService progressService) {
        // Разделение зоны на полигоны  
        var polygons = _spatialElementDividerService.DivideSpatialElement(spatialObject.SpatialElement, _settings, progressService);

        // Получение всех плит перекрытия из настроек
        var allSlabs = _revitRepository.GetSlabsByTypesDocsAndLevels(_settings).ToList();

        // Построение групп колонн
        var columnGroups = _columnFactory.GenerateColumnGroups(polygons, allSlabs, progressService);

        // Финальная сборка
        var geomObjects = _services.GeomObjectsBuildService.GetGeomObjects(_settings, columnGroups, polygons, spatialObject, progressService);

        return geomObjects;
    }
}
