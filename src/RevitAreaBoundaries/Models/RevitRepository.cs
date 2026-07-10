using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitAreaBoundaries.Models.Enums;

using SectionType = RevitAreaBoundaries.Models.Enums.SectionType;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository {
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly IRevitParamFactory _revitParamFactory;

    public RevitRepository(UIApplication uiApplication, SystemPluginConfig systemPluginConfig, IRevitParamFactory revitParamFactory) {
        UiApplication = uiApplication;
        _systemPluginConfig = systemPluginConfig;
        _revitParamFactory = revitParamFactory;
    }

    private UIApplication UiApplication { get; }
    public UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    public IEnumerable<Element> ViewPlans => GetViews();


    public IEnumerable<Element> GetViews() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(vp => !vp.IsTemplate)
            .Where(vp => vp.ViewType == ViewType.AreaPlan);
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
        return GetPlacedElementTypes(_systemPluginConfig.CenterProjectionCats, SectionType.CenterProjection)
            .Concat(GetPlacedElementTypes(_systemPluginConfig.PartialProjectionCats, SectionType.PartialProjection))
            .Concat(GetPlacedElementTypes(_systemPluginConfig.FullProjectionCats, SectionType.FullProjection));
    }

    private IEnumerable<RevitElementType> GetElements(IEnumerable<BuiltInCategory> categories, SectionType sectionType) {
        return categories.SelectMany(category =>
            new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsElementType()
                .Cast<ElementType>()
                .Select(type => new RevitElementType {
                    Element = type,
                    Name = type.Name,
                    CategoryName = GetCategoryName(type),
                    FamilyName = type.FamilyName,
                    SectionType = sectionType 
                })
        );
    }
    
    private IEnumerable<RevitElementType> GetPlacedElementTypes(IEnumerable<BuiltInCategory> categories, SectionType sectionType)
    {
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
                SectionType = sectionType
            });
    }
    
    public string GetGroupNameViewPlan(Element element, RevitParam revitParam) {
        if(revitParam is null) {
            return _systemPluginConfig.DefaultGroupParameterValue;
        }
        string value = element.GetParamValueStringOrDefault(revitParam);
        return string.IsNullOrEmpty(value)
            ? _systemPluginConfig.DefaultGroupParameterValue
            : value;
    }
    
    public IEnumerable<RevitParam> GetBrowserParameters(Element element) {
        var elementIds = GetBrowserParameterElementIds(element);
        
        return elementIds.Count == 0
            ? [] 
            : elementIds.Select(id => _revitParamFactory.Create(Document, id));
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
