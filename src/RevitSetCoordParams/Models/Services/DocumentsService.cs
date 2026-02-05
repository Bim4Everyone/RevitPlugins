using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Services;
internal class DocumentsService : IDocumentsService {
    private readonly Dictionary<string, (Document, Transform)> _documentsByName = [];
    private readonly Document _mainDocument;

    public DocumentsService(Document mainDocument) {
        _mainDocument = mainDocument;
        BuildDocumentsDictionary();
    }

    // Метод поиска документа в словаре по части имени
    public Document GetDocumentByNamePart(string namePart) {
        if(string.IsNullOrWhiteSpace(namePart)) {
            return _mainDocument;
        }
        var koordDoc = _documentsByName
            .FirstOrDefault(dic => dic.Key.Equals(RevitConstants.CoordFilePartName))
            .Value.Item1;
        if(koordDoc != null) {
            return koordDoc;
        }
        var foundDoc = _documentsByName
            .FirstOrDefault(dic => dic.Key.Contains(namePart))
            .Value.Item1;
        return foundDoc ?? _mainDocument;
    }

    // Метод поиска трансформации в словаре по имени
    public Transform GetTransformByName(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        var foundTrans = _documentsByName
            .FirstOrDefault(dic => dic.Key.Equals(name))
            .Value.Item2;
        return foundTrans ?? null;
    }

    // Метод получения всех документов из словаря
    public IEnumerable<Document> GetAllDocuments() {
        return _documentsByName.Values
            .Select(cort => cort.Item1);
    }

    // Метод построения словаря
    private void BuildDocumentsDictionary() {
        _documentsByName.Clear();
        _documentsByName[_mainDocument.Title] = (_mainDocument, null);

        foreach(var linkInst in GetLinkInstances(_mainDocument)) {
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
        .Where(x => x.LinkType != null
                    && !x.LinkType.IsNestedLink
                    && x.LinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
        .Select(x => x.Instance);
    }
}
