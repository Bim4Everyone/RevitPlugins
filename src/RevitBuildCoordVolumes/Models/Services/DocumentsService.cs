using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal class DocumentsService : IDocumentsService {
    private readonly Dictionary<string, Document> _documentsByName = [];

    public DocumentsService(Document mainDocument) {
        BuildDocumentsDictionary(mainDocument);
    }

    // Метод поиска документа в словаре имени
    public Document GetDocumentByName(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        var foundDoc = _documentsByName
            .FirstOrDefault(dic => dic.Key.Equals(name))
            .Value;
        return foundDoc ?? null;
    }

    // Метод получения всех документов из словаря
    public IEnumerable<Document> GetAllDocuments() {
        return _documentsByName.Values;
    }

    // Метод построения словаря
    private void BuildDocumentsDictionary(Document mainDocument) {
        _documentsByName.Clear();
        _documentsByName[mainDocument.Title] = mainDocument;

        foreach(var linkInst in GetLinkInstances(mainDocument)) {
            var doc = linkInst.GetLinkDocument();
            var trans = linkInst.GetTransform();
            if(doc != null && !_documentsByName.ContainsKey(doc.Title)) {
                _documentsByName[doc.Title] = doc;
            }
        }
    }

    // Метод получения всех RevitLinkInstance
    private IEnumerable<RevitLinkInstance> GetLinkInstances(Document doc) {
        var linkInstances = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>();

        return !linkInstances.Any()
            ? Enumerable.Empty<RevitLinkInstance>()
            : linkInstances
        .Select(instance => new {
            Instance = instance,
            LinkType = doc.GetElement(instance.GetTypeId()) as RevitLinkType
        })
        .Where(x => x.LinkType != null
                    && !x.LinkType.IsNestedLink
                    && x.LinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
        .Select(x => x.Instance);
    }

    public Transform GetTransformByName(string name) {
        throw new System.NotImplementedException();
    }
}
