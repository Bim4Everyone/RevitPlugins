using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitParamsChecker.Models.Filtration;

namespace RevitParamsChecker.Models.Revit;

internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;

    public DocumentModel[] GetDocuments() {
        // TODO
        return [new DocumentModel(Document)];
    }

    public DocumentModel GetDocument(string name) {
        throw new System.NotImplementedException(); // TODO
    }

    public ICollection<ElementModel> GetElements(DocumentModel doc, Filter filter) {
        throw new System.NotImplementedException(); // TODO
    }
}
