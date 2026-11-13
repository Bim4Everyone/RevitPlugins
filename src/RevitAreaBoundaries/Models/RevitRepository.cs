using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository(
    UIApplication uiApplication,
    SystemPluginConfig systemPluginConfig,
    IRevitParamFactory revitParamFactory) {
    
    private List<Element> _viewPlans;
    private Transform _basePointTransform;

    private UIApplication UiApplication { get; } = uiApplication;
    private UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Document Document => ActiveUiDocument.Document;
    public Application Application => UiApplication.Application;
    public IReadOnlyList<Element> ViewPlans =>  _viewPlans ??= GetViews();
    public Transform BasePointTransform => _basePointTransform ??= GetBasePointTransform();

    private List<Element> GetViews() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(vp => !vp.IsTemplate)
            .Where(vp => vp.ViewType == ViewType.AreaPlan)
            .ToList<Element>();
    }
    
    // Метод получения коллекции ViewPlans
    public IEnumerable<RevitElement> GetViewPlans() {
        return ViewPlans
            .Select(vp => new RevitElementView {
                Element = vp,
                Name = vp.Name
            });
    }

    public IEnumerable<RevitElementType> GetTypeModels() {
        return GetPlacedElementTypes(systemPluginConfig.CenterProjectionCats, ProjectionType.RegularProjection)
            .Concat(GetPlacedElementTypes(systemPluginConfig.PartialProjectionCats, ProjectionType.PartialProjection))
            .Concat(GetPlacedElementTypes(systemPluginConfig.FullProjectionCats, ProjectionType.FullProjection));
    }

    private Transform GetBasePointTransform() {
        var basePoint = GetBasePointPosition();
        return Transform.CreateTranslation(-basePoint);
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

    private IEnumerable<RevitElementType> GetPlacedElementTypes(IEnumerable<BuiltInCategory> categories, ProjectionType projectionType) {
        var typeIds = categories
            .SelectMany(category =>
                new FilteredElementCollector(Document)
                    .OfCategory(category)
                    .WhereElementIsNotElementType() // только размещённые экземпляры
                    .ToElements()
                    .Select(e => e.GetTypeId())
                    .Where(id => id != ElementId.InvalidElementId))
            .Distinct();

        return typeIds
            .Select(id => Document.GetElement(id))
            .OfType<ElementType>()
            .Select(type => new RevitElementType {
                Element = type,
                Name = type.Name,
                CategoryName = GetCategoryName(type),
                FamilyName = type.FamilyName,
                ProjectionType = projectionType
            });
    }
    
    public IEnumerable<Element> GetElementsOnView(View view, List<RevitElement> revitElements) {
        var elementIds = revitElements
            .Select(x => x.Element.Id)
            .ToHashSet();

        return new FilteredElementCollector(Document, view.Id)
            .WhereElementIsNotElementType()
            .Where(element => ElementMatchesId(element, elementIds));
    }
    
    private static bool ElementMatchesId(Element element, HashSet<ElementId> elementIds) {
        var id = element.GetTypeId();
        return id != ElementId.InvalidElementId && elementIds.Contains(id);
    }

    public string GetGroupNameViewPlan(Element element, RevitParam revitParam) {
        if(revitParam is null) {
            return systemPluginConfig.DefaultGroupParameterValue;
        }
        string value = element.GetParamValueStringOrDefault(revitParam);
        return string.IsNullOrEmpty(value)
            ? systemPluginConfig.DefaultGroupParameterValue
            : value;
    }
    
    public IEnumerable<RevitParam> GetBrowserParameters(Element element) {
        var elementIds = GetBrowserParameterElementIds(element);
        
        return elementIds.Count == 0
            ? [] 
            : elementIds.Select(id => revitParamFactory.Create(Document, id));
    }
    
    private List<ElementId> GetBrowserParameterElementIds(Element element) {
        if(element is null) {
            return [];
        }
        var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForViews(Document);
        var itemsInfo = browserOrganization.GetFolderItems(element.Id);
        
        return itemsInfo.Select(info => info.ElementId).ToList();
    }
    
    private static string GetCategoryName(Element element) {
        var category = element.Category;
        return category?.Name;
    }
    
}
