using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal class DocumentService : IDocumentService {
    private readonly Dictionary<string, (Document, Transform)> _documentsByName = [];

    public DocumentService(Document mainDocument) {
        BuildDocumentsDictionary(mainDocument);
    }

    public Document GetDocumentByName(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        var foundDoc = _documentsByName
            .FirstOrDefault(dic => dic.Key.Equals(name))
            .Value.Item1;
        return foundDoc ?? null;
    }

    public Transform GetTransformByName(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        var foundTrans = _documentsByName
            .FirstOrDefault(dic => dic.Key.Equals(name))
            .Value.Item2;
        return foundTrans ?? null;
    }

    public IEnumerable<Document> GetAllDocuments() {
        return _documentsByName.Values
            .Select(cort => cort.Item1);
    }

    // Метод построения словаря
    private void BuildDocumentsDictionary(Document mainDocument) {
        _documentsByName.Clear();
        _documentsByName[mainDocument.Title] = (mainDocument, null);

        foreach(var linkInst in GetLinkInstances(mainDocument)) {
            var doc = linkInst.GetLinkDocument();
            var trans = linkInst.GetTransform();
            if(doc != null && !_documentsByName.ContainsKey(doc.Title)) {
                _documentsByName[doc.Title] = (doc, trans);
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
        .Where(x => x.LinkType != null && x.LinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
        .Select(x => x.Instance);
    }
}
