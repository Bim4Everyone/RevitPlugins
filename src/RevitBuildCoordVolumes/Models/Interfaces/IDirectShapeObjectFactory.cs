using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IDirectShapeObjectFactory {
    List<DirectShapeObject> GetDirectShapeObjects(List<GeomObject> geomObjects, RevitRepository revitRepository);
}
