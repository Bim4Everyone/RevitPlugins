using System.Collections.Generic;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISlabNormalizeService {
    List<SlabElement> GetNormalizeSlabs(List<SlabElement> slabElements);
}
