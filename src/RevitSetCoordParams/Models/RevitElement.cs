using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;

internal class RevitElement {
    public ElementId Id { get; set; }
    public Element Element { get; set; }
    public Transform Transform { get; set; }
}
