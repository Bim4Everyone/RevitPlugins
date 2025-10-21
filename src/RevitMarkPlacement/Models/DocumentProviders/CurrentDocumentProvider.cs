using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitMarkPlacement.Models.DocumentProviders;

internal sealed class CurrentDocumentProvider : IDocumentProvider {
    private readonly Document _document;
    private readonly UIDocument _uiDocument;

    public CurrentDocumentProvider(UIApplication uiApplication) {
        _document = uiApplication.ActiveUIDocument.Document;
        _uiDocument = uiApplication.ActiveUIDocument;
    }

    public Document GetDocument() {
        return _document;
    }

    public UIDocument GetUIDocument() {
        return _uiDocument;
    }
}
