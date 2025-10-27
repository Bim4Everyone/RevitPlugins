using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;

internal class RevitElement {
    public Element Element { get; set; }
    public Solid Solid { get; set; }
    public BoundingBoxXYZ BoundingBoxXYZ { get; set; }
}
