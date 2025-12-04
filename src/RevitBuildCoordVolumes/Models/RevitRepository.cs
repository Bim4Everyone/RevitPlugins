using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models;

internal class RevitRepository {
    private readonly IDocumentsService _documentsService;
    private readonly ISlabsService _slabsService;

    public RevitRepository(UIApplication uiApp) {
        UIApplication = uiApp;
        _documentsService = new DocumentsService(Document);
        _slabsService = new SlabsService(_documentsService);
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Метод получения всех документов
    /// </summary>
    public IEnumerable<Document> GetAllDocuments() {
        return _documentsService.GetAllDocuments();
    }

    /// <summary>
    /// Метод получения документа по части его имени или по имени целиком
    /// </summary>
    public Document FindDocumentsByName(string name) {
        return _documentsService.GetDocumentByName(name);
    }

    /// <summary>
    /// Получение всех типов перекрытий и фундаментных плит
    /// </summary>
    public IEnumerable<string> GetTypeSlabsByDocs(IEnumerable<Document> documents) {
        return _slabsService.GetSlabsByDocs(documents)
            .Select(slab => slab.Name)
            .Distinct();
    }

    /// <summary>
    /// Получение всех типов перекрытий и фундаментных плит
    /// </summary>
    public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
        return _slabsService.GetSlabsByTypesAndDocs(typeSlabs, documents);
    }


    /// <summary>
    /// Метод получения всех вариантов значений зон по заданному параметру
    /// </summary>
    public IEnumerable<string> GetTypeZones(RevitParam revitParam) {
        return revitParam != null
            ? GetAreas()
                .Select(area => area.GetParamValueOrDefault<string>(revitParam.Name))
                .Where(str => !string.IsNullOrEmpty(str))
                .Distinct()
            : [];

    }

    /// <summary>
    /// Метод получения зон RevitArea по заданному типу и параметру
    /// </summary>
    public IEnumerable<RevitArea> GetRevitAreas(string areaType, RevitParam revitParam) {
        return GetAreas()
            .Select(area => new RevitArea { Area = area })
            .Where(area => areaType.Equals(area.Area.GetParamValueOrDefault<string>(revitParam.Name)));
    }

    // Метод получения системных зон
    private IEnumerable<Area> GetAreas() {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Areas)
            .WhereElementIsNotElementType()
            .OfType<Area>();
    }
}

