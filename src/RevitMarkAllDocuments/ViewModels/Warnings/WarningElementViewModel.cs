using Autodesk.Revit.DB;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class WarningElementViewModel {
    private readonly WarningElement _warningElement;
    public WarningElementViewModel(WarningElement element) {
        _warningElement = element;
    }

    public Element Element => _warningElement.Element;
    public ElementId ElementId => Element.Id;
    public string Name => Element.Name;
    public string ParameterName => _warningElement.ParameterName;
    public string Document => Element.Document.Title;
}
