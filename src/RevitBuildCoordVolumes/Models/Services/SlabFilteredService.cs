using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabFilteredService : ISlabFilteredService{
    public List<SlabElement> FilterSlabBySpatialObject(List<SlabElement> slabs, SpatialObject spatialObject) {
        var spatialCheckingSolid = spatialObject.CheckingSolid;

        return slabs
            .Where(slab => slab.CheckingSolids.Any(solid =>
                SolidUtility.IsIntersect(spatialCheckingSolid, solid)))
            .ToList();
    }
}
