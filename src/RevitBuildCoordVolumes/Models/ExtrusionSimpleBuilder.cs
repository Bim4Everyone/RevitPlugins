using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;
internal class ExtrusionSimpleBuilder : IExtrusionBuilder {
    private readonly IGeomObjectFactory _geomElementFactory;
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;

    public ExtrusionSimpleBuilder(
        IGeomObjectFactory geomElementFactory,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig) {
        _geomElementFactory = geomElementFactory;
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
    }

    public List<GeomObject> BuildVolumes(SpatialObject spatialObject) {

        var spatialElement = spatialObject.SpatialElement;

        double upPosition = _revitRepository.GetPositionInFeet(spatialElement, _systemPluginConfig.UpAreaParamName);
        double bottomPosition = _revitRepository.GetPositionInFeet(spatialElement, _systemPluginConfig.BottomAreaParamName);

        var geomElements = new List<GeomObject>();

        var element = _geomElementFactory.GetSimpleGeomObject(spatialElement, bottomPosition, upPosition);

        if(element != null) {
            geomElements.Add(element);
        }

        return geomElements;
    }
}
