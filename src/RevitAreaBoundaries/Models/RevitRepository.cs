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
    
    
    // Метод получения коллекции ViewPlans
    public IEnumerable<RevitElement> GetViewPlans() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewPlan))              
            .Cast<ViewPlan>()
            .Where(vp => !vp.IsTemplate)
            .Where(vp => vp.ViewType == ViewType.AreaPlan)
            .Select(vp => new RevitElementView {
                Element = vp,
                Name = vp.Name,
                GroupName = GetGroupNameViewPlan(vp)
            });
    }

    public IEnumerable<RevitElement> GetTypeModels() {
        return GetElements(_systemPluginConfig.CenterProjectionCats, SectionType.CenterProjection)
            .Concat(GetElements(_systemPluginConfig.PartialProjectionCats, SectionType.PartialProjection))
            .Concat(GetElements(_systemPluginConfig.FullProjectionCats, SectionType.FullProjection));
    }

    private IEnumerable<RevitElement> GetElements(IEnumerable<BuiltInCategory> categories, SectionType sectionType) {
        return categories.SelectMany(category =>
            new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsElementType()
                .Cast<ElementType>()
                .Select(type => new RevitElementType {
                    Element = type,
                    Name = type.Name,
                    GroupName = GetCategoryName(type),
                    SectionType = sectionType 
                })
        );
    }
    
    private string GetGroupNameViewPlan(Element element) {
        if(element == null) {
            return _systemPluginConfig.DefaultGroupParameterValue;
        }

        var browserParameter = GetBrowserParameter(element);
        if(browserParameter == null) {
            return _systemPluginConfig.DefaultGroupParameterValue;
        }

        string value = element.GetParamValueStringOrDefault(browserParameter);
        return string.IsNullOrEmpty(value)
            ? _systemPluginConfig.DefaultGroupParameterValue
            : value;
    }
    
    private RevitParam GetBrowserParameter(Element element) {
        var elementId = GetBrowserParameterElementId(element);
        
        return elementId == null 
            ? null 
            : _revitParamFactory.Create(Document, elementId);
    }
    
    private ElementId GetBrowserParameterElementId(Element element) {
        if(element is null) {
            return null;
        }
        var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForViews(Document);
        var itemsInfo = browserOrganization.GetFolderItems(element.Id);
        
        return itemsInfo.LastOrDefault()?
            .ElementId;
    }
    
    private static string GetCategoryName(Element element) {
        var category = element.Category;
        return category?.Name;
    }
    
}
