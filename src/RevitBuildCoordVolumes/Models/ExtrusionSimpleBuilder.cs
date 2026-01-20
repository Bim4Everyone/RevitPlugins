using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class ExtrusionSimpleBuilder : IExtrusionBuilder {
    private readonly IGeomObjectFactory _geomElementFactory;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;

    public ExtrusionSimpleBuilder(
        IGeomObjectFactory geomElementFactory,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _geomElementFactory = geomElementFactory;
        _revitRepository = revitRepository;
        _settings = settings;
    }

    public List<GeomObject> BuildVolumes(SpatialObject spatialObject) {
        var spatialElement = spatialObject.SpatialElement;

        var topPositionParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.TopZoneParam)
            .Select(param => param.SourceParam).First();

        var bottomPositionParam = _settings.ParamMaps
            .Where(param => param.Type == ParamType.BottomZoneParam)
            .Select(param => param.SourceParam).First();

        double topPosition = _revitRepository.GetPositionInFeet(spatialElement, topPositionParam.Name);
        double bottomPosition = _revitRepository.GetPositionInFeet(spatialElement, bottomPositionParam.Name);

        var geomElements = new List<GeomObject>();

        var element = _geomElementFactory.GetSimpleGeomObject(spatialElement, bottomPosition, topPosition);

        if(element != null) {
            geomElements.Add(element);
        }

        return geomElements;
    }
}
