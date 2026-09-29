using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitEnumerateBySpline.Models;

internal class RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
    
    private List<SpatialModel>? _allSpatialModels;
    private List<SpatialModel>? _activeViewSpatialModels;
    private List<SpatialModel>? _selectedSpatialModels;
    
    public UIApplication UiApplication { get; } = uiApplication;
    public UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    public List<SpatialModel> AllSpatialModels => _allSpatialModels ??= GetAllSpatialModels();
    public List<SpatialModel> ActiveViewSpatialModels => _activeViewSpatialModels ??= GetActiveViewSpatialModels();
    public List<SpatialModel> SelectedSpatialModels => _selectedSpatialModels ??= GetSelectedSpatialModels();
    
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
        var selected = GetSelectedElements().ToArray();
        if(selected.Length == 0) {
            return [];
        }
        return selected
            .OfType<SpatialElement>()
            .Select(CreateSpatialModel)
            .ToList();
    }
    
    /// <summary>
    /// Метод проверки, есть ли выделенные помещения
    /// </summary>
    public bool HasSelectedRooms() {
        return SelectedSpatialModels.Any();
    }
    
    /// <summary>
    /// Метод проверки, есть ли помещения на активном виде
    /// </summary>
    public bool HasRoomsOnCurrentView() {
        return ActiveViewSpatialModels.Any();
    }
    
    /// <summary>
    /// Метод получения всех помещений 
    /// </summary>
    private List<SpatialElement> GetAllSpatialElements() {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<SpatialElement>()
            .ToList();
    }
    
    // Метод получения всех выделенных элементов модели
    private IEnumerable<Element> GetSelectedElements() {
        return ActiveUiDocument.GetSelectedElements()
            .Where(element => element is Room);
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
