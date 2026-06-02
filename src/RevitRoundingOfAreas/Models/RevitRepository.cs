using System;
using System.Collections.Generic;

using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitRoundingOfAreas.Models;

internal class RevitRepository {
    private readonly ILocalizationService _localizationService;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Основной метод округления площадей
    /// </summary>
    public void RoundingOfAreas(List<SpatialModel> spatialModels, RevitParam sourceParam, RevitParam targetParam, int accuracy) {
        foreach(var spatialModel in spatialModels) {
            double sourceValue = spatialModel.SpatialElement.GetParamValueOrDefault<double>(sourceParam);

            if(sourceValue == default) {
                continue;
            }

            double roundedValue = RoundWithAccuracy(sourceValue, accuracy);

            if(roundedValue == 0) {
                continue;
            }

            spatialModel.SpatialElement.SetParamValue(targetParam, ConvertValueToInternalUnits(roundedValue));
        }
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
    /// Метод получения всех помещений SpatialModel
    /// </summary>
    public List<SpatialModel> GetAllSpatialModels(ElementId phaseId) {
        return GetAllSpatialElements()
             .Where(spatial => ValidatePhase(spatial, phaseId))
             .Select(CreateSpatialModel)
             .ToList();
    }

    /// <summary>
    /// Метод получения всех помещений SpatialModel на активном виде
    /// </summary>
    public List<SpatialModel> GetActiveViewSpatialModels(ElementId phaseId) {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
             .OfCategory(BuiltInCategory.OST_Rooms)
             .WhereElementIsNotElementType()
             .OfType<SpatialElement>()
             .Where(spatial => ValidatePhase(spatial, phaseId))
             .Select(CreateSpatialModel)
             .ToList();
    }

    /// <summary>
    /// Метод получения всех выделенных помещений SpatialModel
    /// </summary>
    public List<SpatialModel> GetSelectedSpatialModels(ElementId phaseId) {
        return !HasSelectedRooms()
            ? []
            : GetSelectedElements()
            .OfType<SpatialElement>()
            .Where(spatial => ValidatePhase(spatial, phaseId))
            .Select(CreateSpatialModel)
            .ToList();
    }

    /// <summary>
    /// Метод проверки, есть ли выделенные помещения
    /// </summary>
    public bool HasSelectedRooms() {
        var selected = GetSelectedElements();
        return selected != null && selected.Count() != 0 && selected
            .Any(element => element is Room);
    }

    /// <summary>
    /// Метод проверки, есть ли помещения на активном виде
    /// </summary>
    public bool HasRoomsOnCurrentView() {
        var viewId = Document.ActiveView.Id;

        return new FilteredElementCollector(Document, viewId)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Any();
    }

    /// <summary>
    /// Метод получения всех выделенных элементов модели
    /// </summary>    
    public IEnumerable<Element> GetSelectedElements() {
        return ActiveUIDocument.GetSelectedElements();
    }

    /// <summary>
    /// Метод выделения элементов в документе
    /// </summary>
    public void SetSelected(ElementId elementId) {
        List<ElementId> listElements = [elementId];
        ActiveUIDocument.SetSelectedElements(listElements);
    }

    /// <summary>
    /// Метод получения стадий проекта
    /// </summary>
    public IEnumerable<PhaseModel> GetPhaseModels() {
        var phases = Document.Phases;
        return phases
            .Cast<Phase>()
            .Select(phase => new PhaseModel {
                ElementId = phase.Id,
                Name = phase.Name
            });
    }

    /// <summary>
    /// Метод получения стадии по имени
    /// </summary>
    public ElementId GetPhaseIdByName(string name) {
        return GetPhaseModels()
            .FirstOrDefault(phase => phase != null && phase.Name == name)
            ?.ElementId
            ?? ElementId.InvalidElementId;
    }

    // Создания объекта SpatialModel
    private SpatialModel CreateSpatialModel(SpatialElement spatialElement) {
        return new SpatialModel {
            SpatialElement = spatialElement,
            LevelName = GetLevelName(spatialElement)
        };
    }

    // Получение уровня помещения
    private string GetLevelName(SpatialElement spatialElement) {
        if(spatialElement is null) {
            _localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        var levelId = spatialElement.LevelId;
        if(levelId is null || levelId == ElementId.InvalidElementId) {
            _localizationService.GetLocalizedString("RevitRepository.NoLevel");
        }
        return Document.GetElement(levelId)?.Name
            ?? _localizationService.GetLocalizedString("RevitRepository.NoLevel");
    }

    // Метод валидации стадии
    private bool ValidatePhase(SpatialElement spatialElement, ElementId phaseId) {
        var spatialPhaseId = spatialElement.get_Parameter(BuiltInParameter.ROOM_PHASE).AsElementId();
        return spatialPhaseId != ElementId.InvalidElementId && spatialPhaseId == phaseId;
    }

    // Метод округления числа с заданной точкой после запятой
    private double RoundWithAccuracy(double? value, int accuracy) {
        return value.HasValue
            ? Math.Round(
                ConvertValueFromInternalUnits(value.Value),
                accuracy,
                MidpointRounding.ToEven)
            : 0;
    }

    // Метод конвертации в миллиметры
    private double ConvertValueFromInternalUnits(double value) {
        return UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.SquareMeters);
    }

    // Метод конвертации в футы
    private double ConvertValueToInternalUnits(double value) {
        return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.SquareMeters);
    }
}
