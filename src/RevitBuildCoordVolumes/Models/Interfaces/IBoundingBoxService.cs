using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Interfaces;

public interface IBoundingBoxService {
    BoundingBoxXYZ GetTransformedBoundingBox(BoundingBoxXYZ bbox, Transform transform);
}
