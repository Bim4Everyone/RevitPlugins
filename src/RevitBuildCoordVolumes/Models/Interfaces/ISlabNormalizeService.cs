using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface ISlabNormalizeService {
    List<Face> GetTopFaces(SlabElement slabElement);
    List<Face> GetTopFacesClean(SlabElement slabElement, List<Face> topFaces);
}
