using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;

internal class DirectShapeObject {
    public DirectShape DirectShape { get; set; }
    public double Volume { get; set; }
}
