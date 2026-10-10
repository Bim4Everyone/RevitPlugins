using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;

public class SolidObject {
    public Solid Solid { get; set; }
    public string LevelName { get; set; }
}
