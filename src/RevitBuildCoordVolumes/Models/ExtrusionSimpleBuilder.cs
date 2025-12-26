using System;
using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;
internal class ExtrusionSimpleBuilder : IExtrusionBuilder {
    public List<GeomElement> BuildVolumes(RevitArea revitArea) {
        throw new NotImplementedException();
    }
}
