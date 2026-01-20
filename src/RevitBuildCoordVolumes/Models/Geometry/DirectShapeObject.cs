using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class DirectShapeObject {
    public DirectShape DirectShape { get; set; }
    public string FloorName { get; set; }
}
