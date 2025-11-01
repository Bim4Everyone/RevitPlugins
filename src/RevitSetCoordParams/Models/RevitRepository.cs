using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Services;

namespace RevitSetCoordParams.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly IDocumentsService _documentsService;
    private readonly WorksetTable _worksetTable;
    private readonly string _defaultFamilyName;
    private readonly string _defaultTypeName;
    private readonly string _defaultLevelName;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _documentsService = new DocumentsService(Document);
        _worksetTable = Document.GetWorksetTable();

        _defaultFamilyName = _localizationService.GetLocalizedString("RevitRepository.DefaultFamilyName");
        _defaultTypeName = _localizationService.GetLocalizedString("RevitRepository.DefaultTypeName");
        _defaultLevelName = _localizationService.GetLocalizedString("RevitRepository.DefaultLevelName");
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Метод получения всех элементов модели по заданным категориям и рабочим наборам
    /// </summary>
    public IEnumerable<RevitElement> GetAllRevitElements(IEnumerable<BuiltInCategory> categories) {
        return categories == null || !categories.Any()
            ? []
            : categories
            .SelectMany(category => new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .Where(element => {
                    var workset = _worksetTable.GetWorkset(element.WorksetId);
                    return !workset.Name.Equals(RevitConstants.WorksetExcludeName);
                })
                .Select(element => new RevitElement {
                    Element = element,
                    BoundingBoxXYZ = element.GetBoundingBox(),
                    FamilyName = GetFamilyName(element),
                    TypeName = element.Name ?? _defaultTypeName,
                    LevelName = GetLevelName(element)
                })
                .Where(element => element.BoundingBoxXYZ != null)
            );
    }

    /// <summary>
    /// Метод получения всех элементов модели на текущем виде по заданным категориям и рабочим наборам
    /// </summary>
    public IEnumerable<RevitElement> GetCurrentViewRevitElements(IEnumerable<BuiltInCategory> categories) {
        return categories == null || !categories.Any()
           ? []
           : categories
           .SelectMany(category => new FilteredElementCollector(Document, GetCurrentView().Id)
               .OfCategory(category)
               .WhereElementIsNotElementType()
               .Where(element => {
                   var workset = _worksetTable.GetWorkset(element.WorksetId);
                   return !workset.Name.Equals(RevitConstants.WorksetExcludeName);
               })
               .Select(element => new RevitElement {
                   Element = element,
                   BoundingBoxXYZ = element.GetBoundingBox(),
                   FamilyName = GetFamilyName(element),
                   TypeName = element.Name ?? _defaultTypeName,
                   LevelName = GetLevelName(element)
               })
               .Where(element => element.BoundingBoxXYZ != null)
           );
    }

    /// <summary>
    /// Метод получения всех выделенных элементов модели  по заданным категориям и рабочим наборам
    /// </summary>
    public IEnumerable<RevitElement> GetSelectedRevitElements(IEnumerable<BuiltInCategory> categories) {
        var selectedElements = GetSelectedElements();

        if(!selectedElements.Any()) {
            return [];
        }
        var categorySet = new HashSet<BuiltInCategory>(categories);
        return selectedElements
            .Where(element => {
                var workset = _worksetTable.GetWorkset(element.WorksetId);
                var builtInCategory = element.Category.GetBuiltInCategory();
                return categorySet.Contains(builtInCategory) && !workset.Name.Equals(RevitConstants.WorksetExcludeName);
            })
            .Select(element => new RevitElement {
                Element = element,
                BoundingBoxXYZ = element.GetBoundingBox(),
                FamilyName = GetFamilyName(element),
                TypeName = element.Name ?? _defaultTypeName,
                LevelName = GetLevelName(element)
            })
            .Where(element => element.BoundingBoxXYZ != null);
    }

    /// <summary>
    /// Метод получения всех выделенных элементов модели  по заданным категориям и рабочим наборам
    /// </summary>    
    public IEnumerable<Element> GetSelectedElements() {
        return ActiveUIDocument.GetSelectedElements();
    }

    /// <summary>
    /// Метод получения объемных элементов по заданному параметру и его значению
    /// </summary>  
    public ICollection<RevitElement> GetRevitElements(Document document, string typeModel) {
        return new FilteredElementCollector(document)
            .OfCategory(RevitConstants.SourceVolumeCategory)
            .WhereElementIsNotElementType()
            .Cast<Element>()
            .Where(instance => instance.GetParamValueString(RevitConstants.SourceVolumeParam).Equals(typeModel))
            .Select(instance => {
                var transform = document.IsLinked ? _documentsService.GetTransformByName(document.GetUniqId()) : null;
                var transSolid = SolidUtils.CreateTransformed(GetUnitedSolid(instance), transform);
                return new RevitElement { Element = instance, Solid = transSolid };
            })
            .ToList();
    }

    /// <summary>
    /// Метод получения все вариантов значений объемных элементов по заданному параметру
    /// </summary>
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

    /// <summary>
    /// Метод получения координаты элементы по центру
    /// </summary>
    public XYZ GetPositionCenter(RevitElement revitElement) {
        return (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
    }

    /// <summary>
    /// Метод получения координаты элементы по низу
    /// </summary>
    public XYZ GetPositionBottom(RevitElement revitElement) {
        var center = (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
        return new XYZ(center.X, center.Y, revitElement.BoundingBoxXYZ.Min.Z);
    }

    /// <summary>
    /// Метод получения координаты элементы по верху
    /// </summary>
    public XYZ GetPositionUp(RevitElement revitElement) {
        var center = (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
        return new XYZ(center.X, center.Y, revitElement.BoundingBoxXYZ.Max.Z);
    }

    /// <summary>
    /// Метод получения документа по части его имени или по имени целиком
    /// </summary>
    public Document FindDocumentsByName(string namePart) {
        return _documentsService.GetDocumentByNamePart(namePart);
    }

    /// <summary>
    /// Метод получения всех документов проекта (текущий + связанные)
    /// </summary>
    public IEnumerable<Document> GetAllDocuments() {
        return _documentsService.GetAllDocuments();
    }

    /// <summary>
    /// Метод получения активного вида
    /// </summary>
    public View GetCurrentView() {
        return ActiveUIDocument.ActiveGraphicalView;
    }

    /// <summary>
    /// Метод получения сферы-солида
    /// </summary>
    public Solid GetSphereSolid(XYZ location, double diameter) {
        var startPoint = new XYZ(location.X, location.Y, location.Z - diameter / 2);
        var midPoint = new XYZ(location.X + diameter / 2, location.Y, location.Z);
        var endPoint = new XYZ(location.X, location.Y, location.Z + diameter / 2);

        var arc = Arc.Create(startPoint, endPoint, midPoint);
        var line = Line.CreateBound(endPoint, startPoint);

        var curve_loop = CurveLoop.Create([arc, line]);

        int startAngle = 0;
        double endAngle = 2 * Math.PI;

        var frame = new Frame { Origin = location };

        return GeometryCreationUtilities.CreateRevolvedGeometry(frame, [curve_loop], startAngle, endAngle);
    }

    /// <summary>
    /// Метод выделения элементов в документе
    /// </summary>
    public void SetSelected(ElementId elementId) {
        List<ElementId> listElements = [elementId];
        ActiveUIDocument.SetSelectedElements(listElements);
    }

    // Метод получения объединенного солида
    private Solid GetUnitedSolid(Element element) {
        var solids = element.GetSolids().ToList();
        var unitedSolids = SolidExtensions.CreateUnitedSolids(solids);

        var validSolids = unitedSolids
            .Where(s => s != null && s.Faces.Size > 0 && s.Edges.Size > 0)
            .ToList();

        return validSolids
            .OrderByDescending(GetSafeSolidVolume)
            .FirstOrDefault();
    }

    // Метод безопасного получения объёма солида
    private double GetSafeSolidVolume(Solid solid) {
        if(solid == null) {
            return 0;
        }
        try {
            return solid.Volume;
        } catch {
            return 0;
        }
    }

    // Метод получения имени семейства
    private string GetFamilyName(Element element) {
        return element is FamilyInstance fi
            ? fi.Symbol?.Family?.Name ?? _defaultFamilyName
            : element.HasElementType()
            ? element.GetElementType()?.FamilyName
            : _defaultFamilyName;
    }

    // Метод получения имени уровня
    private string GetLevelName(Element element) {
        var levelId = element.LevelId;
        if(levelId != ElementId.InvalidElementId) {
            return (Document.GetElement(levelId) as Level)?.Name ?? _defaultLevelName;
        }
        if(element.IsExistsParam(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM)) {
            var scheduleLevelId = element.GetParamValue<ElementId>(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM);
            if(scheduleLevelId.IsNotNull() && scheduleLevelId != ElementId.InvalidElementId) {
                return (Document.GetElement(scheduleLevelId) as Level)?.Name ?? _defaultLevelName;
            }
        }
        return _defaultLevelName;
    }
}
