using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabGeometryService {
    SlabGeometryData GetSlabGeometryData(Floor floor, Transform transformFromDoc);
    Transform CreateZTranslation(double oldPosition, double newPosition);    
}
