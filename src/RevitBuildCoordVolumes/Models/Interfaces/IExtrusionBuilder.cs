using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IExtrusionBuilder {
    List<GeomElement> BuildVolumes(RevitArea revitArea);
}
