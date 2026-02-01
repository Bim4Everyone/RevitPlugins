using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ICoordVolumeBuilder {
    List<GeomObject> Build(SpatialObject spatialElement);
}
