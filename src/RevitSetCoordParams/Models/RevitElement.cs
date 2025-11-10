using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;

internal class RevitElement {
    public Element Element { get; set; }
    public Solid Solid { get; set; }
    public BoundingBoxXYZ BoundingBoxXYZ { get; set; }
    public string FamilyName { get; set; }
    public string TypeName { get; set; }
    public string LevelName { get; set; }
}
