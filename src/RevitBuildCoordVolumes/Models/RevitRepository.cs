using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models;

internal class RevitRepository {
    private readonly IDocumentsService _documentsService;
    private readonly ISlabsService _slabsService;
    private readonly SystemPluginConfig _systemPluginConfig;

    public RevitRepository(UIApplication uiApp, SystemPluginConfig systemPluginConfig) {
        UIApplication = uiApp;
        _systemPluginConfig = systemPluginConfig;
        _documentsService = new DocumentsService(Document);
        _slabsService = new SlabsService(_documentsService, _systemPluginConfig);
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
        return revitParam == null
            ? []
            : GetSpatialElements()
                .Select(area => area.GetParamValueOrDefault<string>(revitParam.Name))
                .Where(str => !string.IsNullOrEmpty(str))
                .Distinct();
    }

    /// <summary>
    /// Метод получения зон RevitArea по заданному типу и параметру
    /// </summary>
    public IEnumerable<SpatialObject> GetSpatialObjects(string areaType, RevitParam revitParam) {
        return GetSpatialElements()
            .Select(spatialElement => new SpatialObject { SpatialElement = spatialElement })
            .Where(spatialObject => areaType.Equals(spatialObject.SpatialElement.GetParamValueOrDefault<string>(revitParam.Name)));
    }

    /// <summary>
    /// Метод получения параметра зоны для выдавливания в простом алгоритме
    /// </summary>
    public double GetPositionInFeet(Element element, string paramName) {
        string positionString = element.GetParamValueOrDefault<string>(paramName);
        return double.TryParse(positionString, out double positionDouble)
            ? UnitUtils.ConvertToInternalUnits(positionDouble, UnitTypeId.Millimeters)
            : double.NaN;
    }

    // Метод получения системных зон или помещений
    private IEnumerable<SpatialElement> GetSpatialElements() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(SpatialElement))
            .WhereElementIsNotElementType()
            .Cast<SpatialElement>()
            .Where(se => se is Area or Room);
    }
}

