using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitParamsChecker.Models.Filtration;

namespace RevitParamsChecker.Models.Revit;

internal class RevitRepository {
    private DocumentModel[] _documentsCache;

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Возвращает коллекцию документов, имеющих отношение к активному документу.
    /// </summary>
    /// <returns>Активный документ и все загруженные связи, по одному экземпляру связи каждого типоразмера связи, если они продублированы.</returns>
    public DocumentModel[] GetDocuments() {
        return _documentsCache ??= [
            new DocumentModel(Document), ..GetRevitLinkInstances().Select(l => new DocumentModel(l))
        ];
    }

    /// <summary>
    /// Возвращает документ по имени без учета регистра
    /// </summary>
    /// <param name="name">Название документа</param>
    public DocumentModel GetDocument(string name) {
        return GetDocuments()
            .FirstOrDefault(d => d.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public ICollection<ElementModel> GetElements(DocumentModel doc, Filter filter) {
        var elementFilter = filter.GetFilter();
        return new FilteredElementCollector(doc.Document)
            .WherePasses(elementFilter)
            .ToElements()
            .Select(e => new ElementModel(
                e,
                doc.IsLink ? new Reference(e).CreateLinkReference(doc.Link) : new Reference(e)))
            .ToArray();
    }

    private ICollection<RevitLinkInstance> GetRevitLinkInstances() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .OfType<RevitLinkInstance>()
            .Where(item => item.GetLinkDocument() != null)
            .GroupBy(l => l.GetLinkDocument().Title)
            .Select(g => g.First())
            .ToArray();
    }
}
