using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.ViewModels;

internal class WarningElementViewModel {
    public Element Element { get; set; }
    public ElementId ElementId => Element.Id;
    public string Name => Element.Name;
    public string ParameterName => Element.Name;
    public string Document => Element.Document.Title;
}
