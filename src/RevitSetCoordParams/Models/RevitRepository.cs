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
    private readonly XYZ _basePointPosition;
    private readonly Transform _localTransform;

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
        _basePointPosition = GetBasePointPosition();
        _localTransform = Transform.CreateTranslation(-_basePointPosition);
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
            .ToList();

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
            .Select(element => {
                var transform = document.IsLinked
                    ? _documentsService.GetTransformByName(document.GetUniqId())
                    : _localTransform;

                var unitedSolid = GetUnitedSolid(element);
                if(unitedSolid is null) {
                    return null;
                }
                var transformedSolid = SolidUtils.CreateTransformed(unitedSolid, transform);
                if(transformedSolid is null) {
                    return null;
                }
                var boundingBox = element.GetBoundingBox();
                if(boundingBox == null) {
                    return null;
                }
                var transformedBoundingBox = GetTransformedBoundingBox(
                    boundingBox,
                    transform.Multiply(boundingBox.Transform)
                );
                var transformedOutline = new Outline(
                    transformedBoundingBox.Min,
                    transformedBoundingBox.Max);

                return new RevitElement {
                    Element = element,
                    Solid = transformedSolid,
                    BoundingBoxXYZ = transformedBoundingBox,
                    Outline = transformedOutline
                };
            })
            .Where(element => element is not null)
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
            BoundingBoxXYZ = GetBoundingBoxXYZ(element),
            FamilyName = GetFamilyName(element),
            TypeName = element.Name ?? _defaultTypeName,
            LevelName = GetLevelName(element)
        };
    }

    // Метод получения BoundingBoxXYZ
    private BoundingBoxXYZ GetBoundingBoxXYZ(Element element) {
        return element is FlexPipe flexPipe
            ? GetFlexElementBoundingBox(flexPipe.Points)
            : element is FlexDuct flexDuct
            ? GetFlexElementBoundingBox(flexDuct.Points)
            : element is FamilyInstance familyInstance
            ? GetFamilyInstanceBoundingBox(familyInstance)
            : element is not FamilyInstance
            ? GetElementBoundingBox(element)
            : null;
    }

    // Метод получения BoundingBoxXYZ для гибких систем
    private BoundingBoxXYZ GetFlexElementBoundingBox(IList<XYZ> points) {
        if(points == null || points.Count == 0) {
            return null;
        }
        var boundingBox = CreatePointsBBox([.. points]);
        return GetTransformedBoundingBox(boundingBox, _localTransform.Multiply(boundingBox.Transform));
    }

    // Метод получения BoundingBoxXYZ из списка точек
    private BoundingBoxXYZ CreatePointsBBox(List<XYZ> points) {
        var (minPoint, maxPoint) = GetMinMaxPoints(points);
        return new BoundingBoxXYZ {
            Min = minPoint,
            Max = maxPoint
        };
    }

    // Метод получения BoundingBoxXYZ загружаемых семейств
    private BoundingBoxXYZ GetFamilyInstanceBoundingBox(FamilyInstance familyInstance) {
        var geomElement = familyInstance.get_Geometry(_geomOptions);
        if(geomElement is null) {
            return null;
        }
        var list = new List<XYZ>();
        foreach(var geomObj in geomElement) {
            if(geomObj is GeometryInstance instance) {
                var instGeom = instance.GetInstanceGeometry();
                foreach(var obj in instGeom) {
                    if(obj is Solid solid && solid.Faces.Size > 0) {
                        foreach(Edge e in solid.Edges) {
                            var st = e.AsCurve().GetEndPoint(0);
                            var fi = e.AsCurve().GetEndPoint(1);
                            list.Add(st);
                            list.Add(fi);
                        }
                    }
                }
            }
        }
        if(list.Count == 0) {
            return null;
        }
        var boundingBox = CreatePointsBBox(list);
        var transformedBoundingBox = GetTransformedBoundingBox(boundingBox, _localTransform.Multiply(boundingBox.Transform));

        return transformedBoundingBox;
    }

    // Метод получения BoundingBoxXYZ системных семейств
    private BoundingBoxXYZ GetElementBoundingBox(Element element) {
        var geomElement = element.get_Geometry(_geomOptions);
        if(geomElement is not null) {
            var geomBoundingBox = geomElement.GetBoundingBox();
            return GetTransformedBoundingBox(geomBoundingBox, _localTransform.Multiply(geomBoundingBox.Transform));
        }
        var boundingBox = element.GetBoundingBox();
        return boundingBox is not null
            ? GetTransformedBoundingBox(boundingBox, _localTransform.Multiply(boundingBox.Transform))
            : null;
    }

    // Метод получения трансформированного BoundingBoxXYZ
    private BoundingBoxXYZ GetTransformedBoundingBox(BoundingBoxXYZ bbox, Transform transform) {
        var min = bbox.Min;
        var max = bbox.Max;

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        for(int x = 0; x <= 1; x++) {
            for(int y = 0; y <= 1; y++) {
                for(int z = 0; z <= 1; z++) {
                    var p = new XYZ(
                        x == 0 ? min.X : max.X,
                        y == 0 ? min.Y : max.Y,
                        z == 0 ? min.Z : max.Z);

                    var tp = transform.OfPoint(p);

                    minX = Math.Min(minX, tp.X);
                    minY = Math.Min(minY, tp.Y);
                    minZ = Math.Min(minZ, tp.Z);

                    maxX = Math.Max(maxX, tp.X);
                    maxY = Math.Max(maxY, tp.Y);
                    maxZ = Math.Max(maxZ, tp.Z);
                }
            }
        }

        return new BoundingBoxXYZ {
            Min = new XYZ(minX, minY, minZ),
            Max = new XYZ(maxX, maxY, maxZ)
        };
    }


    // Метод получения смещения базовой точки    
    private XYZ GetBasePointPosition() {
        var basePoint = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .Cast<BasePoint>()
            .FirstOrDefault();
        return basePoint?.Position;
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
        var solids = element.GetSolids();
        if(!solids.Any() || solids is null) {
            return null;
        }
        var unitedSolids = SolidExtensions.CreateUnitedSolids([.. solids]);
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
