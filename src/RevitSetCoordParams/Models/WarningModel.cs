using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;
internal abstract class WarningModel {
    public WarningDescription WarningDescription { get; set; }
    public Element Element { get; set; }
    public string Caption { get; set; }
}
