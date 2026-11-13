using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Services;

internal interface IDocumentInterface {
    string GetDocumentFullName(Document document);
}
