using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

public abstract class RevitElement {
    public Element Element { get; set; }
    public string Name { get; set; }
}
