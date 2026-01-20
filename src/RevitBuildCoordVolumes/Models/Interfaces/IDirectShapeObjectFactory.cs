using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IDirectShapeObjectFactory {
    List<DirectShapeObject> GetDirectShapeElements(List<GeomObject> geomElements, RevitRepository revitRepository);
}
