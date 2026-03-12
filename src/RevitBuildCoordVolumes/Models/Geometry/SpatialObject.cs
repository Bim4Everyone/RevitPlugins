using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;

internal class SpatialObject {
    public SpatialElement SpatialElement { get; set; }
    public string LevelName { get; set; }
    public string Description { get; set; }
}
