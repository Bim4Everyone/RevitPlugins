using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class ParamBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly IGeomObjectFactory _geomObjectFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;

    public ParamBasedCoordVolumeBuilder(
        RevitRepository revitRepository,
        BuildCoordVolumeServices services,
        BuildCoordVolumeSettings settings) {
        _revitRepository = revitRepository;
        _geomObjectFactory = services.GeomObjectFactory;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject, ProgressService progressService) {
        var spatialElement = spatialObject.SpatialElement;

        var topZoneParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.TopZoneParam)
            .Select(param => param.SourceParam).First();

        var bottomZoneParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.BottomZoneParam)
            .Select(param => param.SourceParam).First();

        double topPosition = _revitRepository.GetPositionInFeet(spatialElement, topZoneParam.Name);
        double bottomPosition = _revitRepository.GetPositionInFeet(spatialElement, bottomZoneParam.Name);

        double basePointOffset = _revitRepository.GetBasePointOffset();

        var listGeomObjects = _geomObjectFactory.GetSimpleGeomObjects(
            spatialElement,
            bottomPosition,
            topPosition,
            basePointOffset,
            progressService);

        return listGeomObjects.Count > 0
            ? listGeomObjects
            : [];
    }
}
