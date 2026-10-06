using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabAnalyzeSlopeService {
    SlabType GetSlabType(Floor floor);
}
