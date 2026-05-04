using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class WarningElement {
    public Element Element { get; set; }
    public string ParameterName { get; set; }
}
