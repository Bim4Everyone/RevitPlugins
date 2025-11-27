using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
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
    private readonly View _view;
    private readonly double _minimalSide;
    private readonly double _side;

    public RevitRepository(UIApplication uiApp) {
        UIApplication = uiApp;
        _documentsService = new DocumentsService(Document);
        _slabsService = new SlabsService();
        _view = Document.ActiveView;
        _minimalSide = Application.ShortCurveTolerance;
        _side = UnitUtils.ConvertToInternalUnits(300, UnitTypeId.SquareMeters);
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
    /// Метод получения координаты элементы по верху
    /// </summary>
    public XYZ GetPositionUp(SlabElement revitElement) {
        return new XYZ(0, 0, 0);
    }

    /// <summary>
    /// Метод получения координаты элементы по низу
    /// </summary>
    public XYZ GetPositionBottom(SlabElement revitElement) {
        return new XYZ(0, 0, 0);
    }

    /// <summary>
    /// Получение всех типов перекрытий и фундаментных плит
    /// </summary>
    public IEnumerable<string> GetTypeSlabs(IEnumerable<Document> documents) {
        return _slabsService.GetSlabsByDocs(documents)
            .Select(slab => slab.Name)
            .Distinct();
    }

    /// <summary>
    /// Получение всех типов перекрытий и фундаментных плит
    /// </summary>
    public IEnumerable<SlabElement> GetSlabs(IEnumerable<string> typeSlabs) {
        return typeSlabs
            .SelectMany(_slabsService.GetSlabsByName);
    }

    /// <summary>
    /// Метод получения всех вариантов значений зон по заданному параметру
    /// </summary>
    public IEnumerable<string> GetTypeZones(RevitParam revitParam) {
        return GetAreas()
            .Select(area => area.GetParamValueOrDefault<string>(revitParam.Name))
            .Where(str => !string.IsNullOrEmpty(str))
            .Distinct();
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
    private IEnumerable<Element> GetAreas() {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Areas)
            .WhereElementIsNotElementType()
            .OfType<Element>();
    }

    public IEnumerable<Element> GetSelectedElements() {
        return ActiveUIDocument.GetSelectedElements();
    }

    public void Process() {
        if(GetSelectedElements().FirstOrDefault() is not Area area) {
            //TaskDialog.Show("Ошибка", "Выберите одну зону (Area).");
            return;
        }

        var divider = new AreaDivider();

        var polygons = divider.DivideArea(area, _side, _minimalSide, 5000);

        Draw(polygons);
    }

    private void Draw(List<Polygon> polygons) {
        using var tr = new Transaction(Document, "Draw");
        tr.Start();
        foreach(var polygon in polygons) {
            foreach(var line in polygon.Sides) {
                Document.Create.NewDetailCurve(_view, line);
            }
        }
        tr.Commit();
    }


}

