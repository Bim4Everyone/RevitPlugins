using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;
internal class DividePlane {
    public Plane PositivePlane { get; set; }
    public Plane NegativePlane { get; set; }
}
