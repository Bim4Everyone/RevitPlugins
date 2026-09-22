using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitEnumerateBySpline.Models;

internal class RevitRepository(
    UIApplication uiApplication, 
    ILocalizationService localizationService) {
    private UIApplication UiApplication { get; } = uiApplication;
    private UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    
    /// <summary>
    /// Метод получения всех помещений SpatialModel
    /// </summary>
    public List<SpatialModel> GetAllSpatialModels() {
        return GetAllSpatialElements()
            .Select(CreateSpatialModel)
            .ToList();
    }
    
    /// <summary>
    /// Метод получения всех помещений SpatialModel на активном виде
    /// </summary>
    public List<SpatialModel> GetActiveViewSpatialModels() {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<SpatialElement>()
            .Select(CreateSpatialModel)
            .ToList();
    }
    
    /// <summary>
    /// Метод получения всех выделенных помещений SpatialModel
    /// </summary>
    public List<SpatialModel> GetSelectedSpatialModels() {
        return !HasSelectedRooms()
            ? []
            : GetSelectedElements()
                .OfType<SpatialElement>()
                .Select(CreateSpatialModel)
                .ToList();
    }
    
    /// <summary>
    /// Метод получения всех помещений 
    /// </summary>
    public List<SpatialElement> GetAllSpatialElements() {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<SpatialElement>()
            .ToList();
    }
    
    
    /// <summary>
    /// Метод проверки, есть ли выделенные помещения
    /// </summary>
    public bool HasSelectedRooms() {
        var selected = GetSelectedElements().ToArray();
        return selected.Count() != 0
               && selected.Any(element => element is Room);
    }
    
    // Метод получения всех выделенных элементов модели
    private IEnumerable<Element> GetSelectedElements() {
        return ActiveUiDocument.GetSelectedElements();
    }
    
    
    // Создания объекта SpatialModel
    private SpatialModel CreateSpatialModel(SpatialElement spatialElement) {
        return new SpatialModel {
            SpatialElement = spatialElement,
            LevelName = GetLevelName(spatialElement)
        };
    }
    
    // Получение уровня помещения
    private string GetLevelName(SpatialElement? spatialElement) {
        if(spatialElement is null) {
            localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        var levelId = spatialElement?.LevelId;
        if(levelId == ElementId.InvalidElementId) {
            localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        return Document.GetElement(levelId)?.Name
               ?? localizationService.GetLocalizedString("RevitRepository.NoLevel");
    }
}
