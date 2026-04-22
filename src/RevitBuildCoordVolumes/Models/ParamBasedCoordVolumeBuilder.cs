using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class ParamBasedCoordVolumeBuilder : ICoordVolumeBuilder {
    private readonly IGeomObjectFactory _geomObjectFactory;
    private readonly BuildCoordVolumeSettings _settings;

    public ParamBasedCoordVolumeBuilder(
        BuildCoordVolumeServices services,
        BuildCoordVolumeSettings settings) {
        _geomObjectFactory = services.GeomObjectFactory;
        _settings = settings;
    }

    public List<GeomObject> Build(SpatialObject spatialObject, ProgressService progressService) {
        var listGeomObjects = _geomObjectFactory.GetSimpleGeomObjects(_settings, spatialObject, progressService);

        return listGeomObjects.Count > 0
            ? listGeomObjects
            : [];
    }
}
