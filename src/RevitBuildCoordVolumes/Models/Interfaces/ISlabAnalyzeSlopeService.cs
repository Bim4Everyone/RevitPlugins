using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface ISlabAnalyzeSlopeService {
    bool IsSloped(Floor floor);
}
