using System.Collections.Generic;
using System.Linq;


using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitSetCoordParams.Models;

internal class RevitRepository {

    // Словарь всех связанных документов, включая текущий
    private readonly Dictionary<string, Document> _documentsByName = [];

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
        BuildDocumentsDictionary();
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public ICollection<RevitElement> GetAllRevitElements(IEnumerable<BuiltInCategory> categories) {
        return [];
    }

    public ICollection<RevitElement> GetCurrentViewRevitElements(IEnumerable<BuiltInCategory> categories) {
        return [];
    }

    public ICollection<RevitElement> GetSelectedRevitElements(IEnumerable<BuiltInCategory> categories) {
        return [];
    }

    public View GetCurrentView() {
        return ActiveUIDocument.ActiveView;
    }

    // Метод получения всех значений параметра ФОП_Зона у категории "Обобщенные модели"
    public IEnumerable<string> GetSourceElementsValues(Document document) {
        var collector = new FilteredElementCollector(document)
            .OfCategory(RevitConstants.SourceVolumeCategory)
            .Cast<Element>();

        return !collector.Any()
            ? []
            : collector
                .Select(sourceVolume => sourceVolume.GetParamValueOrDefault<string>(RevitConstants.SourceVolumeParam.Name))
                .Where(s => !string.IsNullOrEmpty(s) && s.Contains(RevitConstants.TypeModelPartName))
                .Distinct();
    }

    // Метод возвращения документа по имени
    public Document FindDocumentsByName(string namePart) {
        if(string.IsNullOrWhiteSpace(namePart)) {
            return Document;
        }
        var koordDoc = _documentsByName
            .FirstOrDefault(kvp => kvp.Key.Equals(RevitConstants.CoordFilePartName))
            .Value;
        if(koordDoc != null) {
            return koordDoc;
        }
        var foundDoc = _documentsByName
            .FirstOrDefault(kvp => kvp.Key.Contains(namePart))
            .Value;
        return foundDoc ?? Document;
    }

    // Метод возвращения всех документов, включая текущий
    public IEnumerable<Document> GetAllDocuments() {
        return _documentsByName
            .Select(pair => pair.Value);
    }

    // Метод заполнения словаря (имя, документ)
    private void BuildDocumentsDictionary() {
        _documentsByName.Clear();

        if(Document != null) {
            _documentsByName[Document.Title] = Document;
        }

        foreach(var linkDoc in GetLinkDocuments()) {
            if(linkDoc != null && !_documentsByName.ContainsKey(linkDoc.Title)) {
                _documentsByName[linkDoc.Title] = linkDoc;
            }
        }
    }

    // Метод получения документа "Связанный файл"
    private IEnumerable<Document> GetLinkDocuments() {
        var linkInstances = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>();

        return !linkInstances.Any()
            ? Enumerable.Empty<Document>()
            : linkInstances
        .Select(instance => new {
            Instance = instance,
            LinkType = Document.GetElement(instance.GetTypeId()) as RevitLinkType
        })
        .Where(x => x.LinkType != null
                    && !x.LinkType.IsNestedLink
                    && x.LinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
        .Select(x => x.Instance.GetLinkDocument())
        .Where(doc => doc != null);
    }
}
