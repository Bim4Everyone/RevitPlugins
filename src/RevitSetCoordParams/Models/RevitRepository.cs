using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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
    private readonly IDependentElementService _dependentService;
    private readonly WorksetTable _worksetTable;
    private readonly string _defaultFamilyName;
    private readonly string _defaultTypeName;
    private readonly string _defaultLevelName;
    private readonly Options _geomOptions;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
        _documentsService = new DocumentsService(Document);
        _dependentService = new DependentElementService(Document);
        _worksetTable = Document.GetWorksetTable();

        _defaultFamilyName = _localizationService.GetLocalizedString("RevitRepository.DefaultFamilyName");
        _defaultTypeName = _localizationService.GetLocalizedString("RevitRepository.DefaultTypeName");
        _defaultLevelName = _localizationService.GetLocalizedString("RevitRepository.DefaultLevelName");

        _geomOptions = new Options {
            ComputeReferences = true,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Метод получения всех элементов модели по заданным категориям и рабочим наборам
    /// </summary> 
    public List<RevitElement> GetAllRevitElements(ICollection<BuiltInCategory> categories) {
        if(categories == null || categories.Count == 0) {
            return [];
        }
        var categorySet = new HashSet<BuiltInCategory>(categories);

        var allElements = new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .Where(element => ElementMatchesCategoryAndWorkSet(element, categorySet));

        var result = allElements
            .Where(_dependentService.IsRootElement)
            .Select(e => {
                var revitElement = CreateRevitElement(e);
                revitElement.DependentElements = GetDependentElements(e, categorySet);
                return revitElement;
            })
            .Where(e => e.BoundingBoxXYZ != null)
            .ToList();

        return result;
    }

    /// <summary>
    /// Метод получения всех элементов модели на текущем виде по заданным категориям и рабочим наборам
    /// </summary>
    public List<RevitElement> GetCurrentViewRevitElements(ICollection<BuiltInCategory> categories) {
        if(categories == null || categories.Count == 0) {
            return [];
        }

        var categorySet = new HashSet<BuiltInCategory>(categories);

        var allElements = new FilteredElementCollector(Document, GetCurrentView().Id)
           .WhereElementIsNotElementType()
           .Where(element => ElementMatchesCategoryAndWorkSet(element, categorySet));

        var result = allElements
            .Where(_dependentService.IsRootElement)
            .Select(e => {
                var revitElement = CreateRevitElement(e);
                revitElement.DependentElements = GetDependentElements(e, categorySet);
                return revitElement;
            })
            .Where(e => e.BoundingBoxXYZ != null)
            .ToList();

        return result;
    }

    /// <summary>
    /// Метод получения всех выделенных элементов модели по заданным категориям и рабочим наборам, включая только родительские семейства
    /// </summary>
    public List<RevitElement> GetSelectedRevitElements(ICollection<BuiltInCategory> categories) {
        var selectedElements = GetSelectedElements();
        if(selectedElements == null || selectedElements.Count() == 0) {
            return [];
        }

        var categorySet = new HashSet<BuiltInCategory>(categories);

        var result = selectedElements
            .Where(element => element != null)
            .Where(element => ElementMatchesCategoryAndWorkSet(element, categorySet))
            .GroupBy(element => element.Id)
            .Select(group => group.First())
            .Select(element => {
                var revitElement = CreateRevitElement(element);
                revitElement.DependentElements = GetDependentElements(element, categorySet);
                return revitElement;
            })
            .Where(revitElement => revitElement.BoundingBoxXYZ != null)
            .ToList();
        ;

        return result;
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
            .Where(instance => {
                string value = instance.GetParamValueOrDefault<string>(RevitConstants.SourceVolumeParam);
                return value != null && value.Equals(typeModel);
            })
            .Select(instance => {
                var transform = document.IsLinked
                    ? _documentsService.GetTransformByName(document.GetUniqId())
                    : null;
                var unitedSolid = GetUnitedSolid(instance);
                var transSolid = transform != null
                    ? SolidUtils.CreateTransformed(unitedSolid, transform)
                    : unitedSolid;
                var boundingBox = transSolid.GetBoundingBox();
                var outline = new Outline(boundingBox.Transform.OfPoint(boundingBox.Min), boundingBox.Transform.OfPoint(boundingBox.Max));
                return new RevitElement {
                    Element = instance,
                    Solid = transSolid,
                    BoundingBoxXYZ = boundingBox,
                    Outline = outline
                };
            })
            .ToList();
    }

    /// <summary>
    /// Метод получения всех вариантов значений объемных элементов по заданному параметру
    /// </summary>
    public IEnumerable<string> GetSourceElementsValues(Document document) {
        var collector = new FilteredElementCollector(document)
            .OfCategory(RevitConstants.SourceVolumeCategory)
            .Cast<Element>();

        return !collector.Any()
            ? []
            : collector
                .Select(sourceVolume => sourceVolume.GetParamValueOrDefault<string>(RevitConstants.SourceVolumeParam.Name))
                .Where(s => !string.IsNullOrEmpty(s))
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
    /// Метод выделения элементов в документе
    /// </summary>
    public void SetSelected(List<ElementId> elementIds) {
        ActiveUIDocument.SetSelectedElements(elementIds);
    }

    // Метод создания RevitElement
    private RevitElement CreateRevitElement(Element element) {
        return new RevitElement {
            Element = element,
            BoundingBoxXYZ = GetGeometricalBoundingBoxXYZ(element),
            FamilyName = GetFamilyName(element),
            TypeName = element.Name ?? _defaultTypeName,
            LevelName = GetLevelName(element)
        };
    }

    // Метод получения GetBoundingBoxXYZ
    private BoundingBoxXYZ GetGeometricalBoundingBoxXYZ(Element element) {
        if(element is FlexPipe fp) {
            return CreatePointsBBox([.. fp.Points]);
        }
        if(element is FlexDuct fd) {
            return CreatePointsBBox([.. fd.Points]);
        }
        if(element is FamilyInstance fi) {
            var tr = fi.GetTransform();
            return GetGeomBoundingBox(fi, tr);
        }
        var geom = element.get_Geometry(_geomOptions);
        if(geom != null) {
            return geom.GetBoundingBox();
        }
        var bbox = element.GetBoundingBox();
        return bbox ?? null;
    }

    // Метод получения BoundingBoxXYZ геометрической части элемента
    private BoundingBoxXYZ GetGeomBoundingBox(Element element, Transform transform) {
        var geomElement = element.get_Geometry(_geomOptions);
        if(geomElement == null) {
            return null;
        }
        var minPoint = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
        var maxPoint = new XYZ(double.MinValue, double.MinValue, double.MinValue);

        var minP = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
        var maxP = new XYZ(double.MinValue, double.MinValue, double.MinValue);

        foreach(var geomObj in geomElement) {
            if(geomObj is GeometryInstance instance) {
                var instGeom = instance.GetInstanceGeometry();

                foreach(var obj in instGeom) {
                    if(obj is Solid solid && solid.Faces.Size > 0) {
                        var bbox = solid.GetBoundingBox();

                        var min = transform.OfPoint(bbox.Min);
                        var max = transform.OfPoint(bbox.Max);

                        minPoint = new XYZ(
                            Math.Min(minPoint.X, min.X),
                            Math.Min(minPoint.Y, min.Y),
                            Math.Min(minPoint.Z, min.Z)
                        );
                        maxPoint = new XYZ(
                            Math.Max(maxPoint.X, max.X),
                            Math.Max(maxPoint.Y, max.Y),
                            Math.Max(maxPoint.Z, max.Z)
                        );
                    }
                }
            }
        }
        return minPoint.X == double.MaxValue ? null : new BoundingBoxXYZ { Min = minPoint, Max = maxPoint };
    }

    // Метод получения BoundingBoxXYZ из списка точек
    private BoundingBoxXYZ CreatePointsBBox(List<XYZ> points) {
        var (minPoint, maxPoint) = GetMinMaxPoints(points);
        return new BoundingBoxXYZ {
            Min = minPoint,
            Max = maxPoint
        };
    }

    // Метод получения минимальной и максимальной точек из списка точек
    private (XYZ minPoint, XYZ maxPoint) GetMinMaxPoints(List<XYZ> points) {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double minZ = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        double maxZ = double.MinValue;

        foreach(var point in points) {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            minZ = Math.Min(minZ, point.Z);

            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
            maxZ = Math.Max(maxZ, point.Z);
        }
        var min = new XYZ(minX, minY, minZ);
        var max = new XYZ(maxX, maxY, maxZ);
        return (min, max);
    }

    // Метод получения вложенных/зависимых RevitElement
    private List<RevitElement> GetDependentElements(Element element, HashSet<BuiltInCategory> categorySet) {
        var result = new List<RevitElement>();
        if(!_dependentService.DependentMap.TryGetValue(element.Id, out var dependentIds)) {
            return result;
        }
        foreach(var id in dependentIds) {
            var depElement = Document.GetElement(id);
            if(depElement == null) {
                continue;
            }
            if(!ElementMatchesCategoryAndWorkSet(depElement, categorySet)) {
                continue;
            }
            var revitElement = CreateRevitElement(depElement);
            if(revitElement?.BoundingBoxXYZ == null) {
                continue;
            }
            result.Add(revitElement);
        }
        return result;
    }

    // Метод фильтрации элементов по рабочему набору и категориям
    private bool ElementMatchesCategoryAndWorkSet(Element element, HashSet<BuiltInCategory> categories) {
        var category = element.Category?.GetBuiltInCategory();
        return category != null && categories.Contains(category.Value) && ElementOutWorkset(element);
    }

    // Метод фильтрации элементов по рабочему набору
    private bool ElementOutWorkset(Element element) {
        var workset = _worksetTable.GetWorkset(element.WorksetId);
        return !workset.Name.Equals(RevitConstants.WorksetExcludeName);
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
