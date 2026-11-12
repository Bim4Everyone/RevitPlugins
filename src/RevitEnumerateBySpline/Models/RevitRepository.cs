using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitEnumerateBySpline.Models;

/// <summary>Предоставляет доступ к помещениям и документу Revit.</summary>
internal class RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
    
    private List<SpatialModel>? _allSpatialModels;
    private List<SpatialModel>? _activeViewSpatialModels;
    private List<SpatialModel>? _selectedSpatialModels;
    
    public UIApplication UiApplication { get; } = uiApplication;
    public UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Document Document => ActiveUiDocument.Document;
    public List<SpatialModel> AllSpatialModels => _allSpatialModels ??= GetAllSpatialModels();
    public List<SpatialModel> ActiveViewSpatialModels => _activeViewSpatialModels ??= GetActiveViewSpatialModels();
    public List<SpatialModel> SelectedSpatialModels => _selectedSpatialModels ??= GetSelectedSpatialModels();
    
    /// <summary>
    /// Метод получения всех помещений SpatialModel
    /// </summary>
    /// <returns>Помещения документа.</returns>
    public List<SpatialModel> GetAllSpatialModels() {
        return GetAllSpatialElements()
            .Select(CreateSpatialModel)
            .ToList();
    }
    
    /// <summary>
    /// Метод получения всех помещений SpatialModel на активном виде
    /// </summary>
    /// <returns>Помещения активного вида.</returns>
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
    /// <returns>Выделенные помещения.</returns>
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
    /// <returns>Истина, если помещения выделены.</returns>
    public bool HasSelectedRooms() {
        return SelectedSpatialModels.Any();
    }
    
    /// <summary>
    /// Метод проверки, есть ли помещения на активном виде
    /// </summary>
    /// <returns>Истина, если на активном виде есть помещения.</returns>
    public bool HasRoomsOnCurrentView() {
        return ActiveViewSpatialModels.Any();
    }

    /// <summary>Выделяет помещение в активном документе.</summary>
    /// <param name="elementId">Идентификатор выделяемого помещения.</param>
    public void SetSelected(ElementId elementId) {
        ActiveUiDocument.SetSelectedElements([elementId]);
    }
    
    // Получает все помещения документа.
    private List<SpatialElement> GetAllSpatialElements() {
        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<SpatialElement>()
            .ToList();
    }
    
    // Получает выделенные элементы.
    private IEnumerable<Element> GetSelectedElements() {
        return ActiveUiDocument.GetSelectedElements()
            .Where(element => element is Room);
    }
    
    // Создаёт модель помещения.
    private SpatialModel CreateSpatialModel(SpatialElement spatialElement) {
        return new SpatialModel {
            SpatialElement = spatialElement,
            LevelName = GetLevelName(spatialElement)
        };
    }
    
    // Получает уровень помещения.
    private string GetLevelName(SpatialElement? spatialElement) {
        if(spatialElement is null) {
            return localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        var levelId = spatialElement.LevelId;
        if(levelId == ElementId.InvalidElementId) {
            return localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        return Document.GetElement(levelId)?.Name
               ?? localizationService.GetLocalizedString("RevitRepository.NoLevel");
    }
}
