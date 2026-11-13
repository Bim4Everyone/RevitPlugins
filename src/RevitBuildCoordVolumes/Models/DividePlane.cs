using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

internal class DividePlane {
    public Plane PositivePlane { get; set; }
    public Plane NegativePlane { get; set; }
}
