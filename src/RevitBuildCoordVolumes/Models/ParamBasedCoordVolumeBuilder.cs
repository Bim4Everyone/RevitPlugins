using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class ParamBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly IGeomObjectFactory _geomElementFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;

    public ParamBasedCoordVolumeBuilder(
        IGeomObjectFactory geomElementFactory,
        RevitRepository revitRepository,
        BuildCoordVolumeSettings settings) {
        _geomElementFactory = geomElementFactory;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject) {
        var spatialElement = spatialObject.SpatialElement;

        var topZoneParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.TopZoneParam)
            .Select(param => param.SourceParam).First();

        var bottomZoneParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.BottomZoneParam)
            .Select(param => param.SourceParam).First();

        double topPosition = _revitRepository.GetPositionInFeet(spatialElement, topZoneParam.Name);
        double bottomPosition = _revitRepository.GetPositionInFeet(spatialElement, bottomZoneParam.Name);

        var listGeomObjects = _geomElementFactory.GetSimpleGeomObjects(spatialElement, bottomPosition, topPosition);

        return listGeomObjects.Count > 0
            ? listGeomObjects
            : [];
    }
}
