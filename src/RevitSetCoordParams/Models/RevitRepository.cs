using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitSetCoordParams.Models.Services;

namespace RevitSetCoordParams.Models;

internal class RevitRepository {

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    private DocumentsService DocumentsService => new(Document);


    public IEnumerable<RevitElement> GetAllRevitElements(IEnumerable<BuiltInCategory> categories) {
        return categories == null || !categories.Any()
            ? []
            : categories
            .SelectMany(category => new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .Cast<Element>()
                .Select(element => new RevitElement {
                    Element = element,
                    BoundingBoxXYZ = element.GetBoundingBox()
                })
                .Where(element => element.BoundingBoxXYZ != null)
            );
    }

    public IEnumerable<RevitElement> GetCurrentViewRevitElements(IEnumerable<BuiltInCategory> categories) {
        return categories == null || !categories.Any()
           ? []
           : categories
           .SelectMany(category => new FilteredElementCollector(Document, GetCurrentView().Id)
               .OfCategory(category)
               .WhereElementIsNotElementType()
               .Cast<Element>()
               .Select(element => new RevitElement {
                   Element = element,
                   BoundingBoxXYZ = element.GetBoundingBox()
               })
               .Where(element => element.BoundingBoxXYZ != null)
           );
    }

    public IEnumerable<RevitElement> GetSelectedRevitElements(IEnumerable<BuiltInCategory> categories) {
        var selectedElements = ActiveUIDocument.GetSelectedElements();

        if(!selectedElements.Any()) {
            return [];
        }
        var categorySet = new HashSet<BuiltInCategory>(categories);
        return selectedElements
            .Where(element => {
                var builtInCategory = element.Category.GetBuiltInCategory();
                return categorySet.Contains(builtInCategory);
            })
            .Select(element => new RevitElement {
                Element = element,
                BoundingBoxXYZ = element.GetBoundingBox()
            })
            .Where(element => element.BoundingBoxXYZ != null);
    }

    // Метод получения выделенных элементов
    public IEnumerable<Element> GetSelectedElements() {
        return ActiveUIDocument.GetSelectedElements();
    }

    public ICollection<RevitElement> GetRevitElements(Document document, string typeModel) {
        return new FilteredElementCollector(document)
            .OfCategory(RevitConstants.SourceVolumeCategory)
            .WhereElementIsNotElementType()
            .Cast<Element>()
            .Where(instance => instance.GetParamValueString(RevitConstants.SourceVolumeParam).Equals(typeModel))
            .Select(instance => {
                var transform = document.IsLinked ? DocumentsService.GetTransformByName(document.GetUniqId()) : null;
                var transSolid = SolidUtils.CreateTransformed(GetUnitedSolid(instance), transform);
                return new RevitElement { Element = instance, Solid = transSolid };
            })
            .ToList();
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

    public XYZ GetPositionCenter(RevitElement revitElement) {
        return (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
    }

    public XYZ GetPositionBottom(RevitElement revitElement) {
        var center = (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
        return new XYZ(center.X, center.Y, revitElement.BoundingBoxXYZ.Min.Z);
    }

    public XYZ GetPositionUp(RevitElement revitElement) {
        var center = (revitElement.BoundingBoxXYZ.Max + revitElement.BoundingBoxXYZ.Min) / 2;
        return new XYZ(center.X, center.Y, revitElement.BoundingBoxXYZ.Max.Z);
    }

    // Метод возвращения документа по имени
    public Document FindDocumentsByName(string namePart) {
        return DocumentsService.GetDocumentByNamePart(namePart);
    }

    // Метод возвращения всех документов, включая текущий
    public IEnumerable<Document> GetAllDocuments() {
        return DocumentsService.GetAllDocuments();
    }

    public View GetCurrentView() {
        return ActiveUIDocument.ActiveGraphicalView;
    }

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
}
