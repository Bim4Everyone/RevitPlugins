using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.Models.Calculation;
using RevitRooms.Services;
using RevitRooms.Views;

namespace RevitRooms.ViewModels.Rooms;
internal abstract class RevitRoomsViewModel : BaseViewModel {
    public Guid _id;
    protected readonly RevitRepository _revitRepository;
    protected readonly RoomsConfig _roomsConfig;
    protected readonly IMessageBoxService _messageBoxService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ErrorWindowService _errorWindowService;

    private string _errorText;
    private bool _isAllowSelectLevels;
    private bool _isFillLevel;

    public RevitRoomsViewModel(RevitRepository revitRepository, 
                               RoomsConfig roomsConfig,
                               IMessageBoxService messageBoxService,
                               ILocalizationService localizationService,
                               ErrorWindowService errorWindowService) {
        _revitRepository = revitRepository;
        _roomsConfig = roomsConfig;
        _messageBoxService = messageBoxService;
        _localizationService = localizationService;
        _errorWindowService = errorWindowService;

        Levels = [.. GetLevelViewModels()
            .OrderBy(item => item.Element.Elevation)
            .Where(item => item.SpatialElements.Count > 0)];
        AdditionalPhases = [.. _revitRepository
            .GetAdditionalPhases()
            .Select(item => new PhaseViewModel(item, _revitRepository))];
        Phases = [.. Levels
            .SelectMany(item => item.SpatialElements)
            .Select(item => item.Phase)
            .Where(item => item != null)
            .Distinct()
            .Except(AdditionalPhases)];
        Phase = Phases.FirstOrDefault();

        RoundAccuracy = 1;
        RoundAccuracyValues = [.. Enumerable.Range(1, 3)];

        CalculateCommand = new RelayCommand(Calculate, CanCalculate);
        CalculateAreasCommand = new RelayCommand(CalculateAreas, CanCalculateAreas);

        // Установка конфигурации
        SetRoomsConfig();
    }

    public ICommand CalculateCommand { get; }
    public ICommand CalculateAreasCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool IsAllowSelectLevels {
        get => _isAllowSelectLevels;
        set => RaiseAndSetIfChanged(ref _isAllowSelectLevels, value);
    }

    public bool IsFillLevel {
        get => _isFillLevel;
        set => RaiseAndSetIfChanged(ref _isFillLevel, value);
    }

    public string Name { get; set; }
    public PhaseViewModel Phase { get; set; }

    public bool NotShowWarnings { get; set; }
    public bool IsCountRooms { get; set; }
    public bool IsSpotCalcArea { get; set; }
    public bool IsCheckRoomsChanges { get; set; }
    public string RoomAccuracy { get; set; } = "100";

    public int RoundAccuracy { get; set; }
    public ObservableCollection<int> RoundAccuracyValues { get; }

    public ObservableCollection<PhaseViewModel> Phases { get; }
    public ObservableCollection<LevelViewModel> Levels { get; }
    public ObservableCollection<PhaseViewModel> AdditionalPhases { get; }

    private List<WarningViewModel> Warnings { get; set; } = [];

    protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();

    private void SetRoomsConfig() {
        var settings = _roomsConfig.GetSettings(_revitRepository.Document);
        if(settings == null) {
            return;
        }

        IsFillLevel = settings.IsFillLevel;
        NotShowWarnings = settings.NotShowWarnings;
        IsCountRooms = settings.IsCountRooms;
        IsSpotCalcArea = settings.IsSpotCalcArea;
        IsCheckRoomsChanges = settings.IsCheckRoomsChanges;

        RoomAccuracy = settings.RoomAccuracy;
        RoundAccuracy = settings.RoundAccuracy;

        if(_revitRepository.GetElement(settings.PhaseElementId) is Phase phase) {
            if(!(phase == null || Phase?.ElementId == phase.Id)) {
                Phase = Phases.FirstOrDefault(item => item.ElementId == phase.Id) ?? Phases.FirstOrDefault();
            }

        }

        foreach(var level in Levels.Where(item => settings.Levels.Contains(item.ElementId))) {
            level.IsSelected = true;
        }
    }

    private void SaveRoomsConfig() {
        var settings = _roomsConfig.GetSettings(_revitRepository.Document);
        settings ??= _roomsConfig.AddSettings(_revitRepository.Document);

        settings.IsFillLevel = IsFillLevel;
        settings.NotShowWarnings = NotShowWarnings;
        settings.IsCountRooms = IsCountRooms;
        settings.IsSpotCalcArea = IsSpotCalcArea;
        settings.IsCheckRoomsChanges = IsCheckRoomsChanges;

        settings.RoomAccuracy = RoomAccuracy;
        settings.RoundAccuracy = RoundAccuracy;

        settings.SelectedRoomId = _id;
        settings.PhaseElementId = Phase.ElementId;

        settings.Levels = Levels.Where(item => item.IsSelected).Select(item => item.ElementId).ToList();

        _roomsConfig.SaveProjectConfig();
    }

    private void CalculateAreas(object p) {
        // Удаляем все не размещенные помещения
        _revitRepository.RemoveUnplacedSpatialElements();

        // Обрабатываем все зоны
        var errorElements = new Dictionary<string, WarningViewModel>();
        var redundantAreas = GetAreas().Where(item => item.IsRedundant == true || item.NotEnclosed == true);
        AddElements(WarningInfo.GetRedundantAreas(_localizationService), redundantAreas, errorElements);

        Warnings = errorElements.Values.ToList();
        if(Warnings.Count > 0) {
            _errorWindowService.ShowNoticeWindow(NotShowWarnings, Warnings);
            return;
        }

        // получаем уже обработанные имена уровней
        var levelNames = _revitRepository.GetLevelNames();

        var bigChangesRooms = new Dictionary<string, WarningViewModel>();
        string transactionName = _localizationService.GetLocalizedString("Transaction.CalculateAreas");
        using(var transaction = _revitRepository.Document.StartTransaction(transactionName)) {
            // Надеюсь будет достаточно быстро отрабатывать :)
            // Обновление параметра округления у зон
            foreach(var spatialElement in GetAreas()) {
                if(IsFillLevel) {
                    // Заполняем параметр Этаж
                    _revitRepository.UpdateLevelSharedParam(spatialElement.Element, levelNames);
                }

                // Обновление параметра
                // площади с коэффициентом

                // Расчет площади
                var area = new RoomAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase?.Element };
                area.CalculateParam(spatialElement);
                bool isChangedArea = area.SetParamValue(spatialElement);

                // Площадь с коэффициентом зависит от площади
                var areaWithRatio = new AreaWithRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase?.Element };
                areaWithRatio.CalculateParam(spatialElement);
                areaWithRatio.SetParamValue(spatialElement);

                if(isChangedArea && IsCheckRoomsChanges) {
                    double differences = areaWithRatio.GetDifferences();
                    double percentChange = areaWithRatio.GetPercentChange();
                    AddElement(WarningInfo.GetBigChangesAreas(_localizationService), FormatMessage(differences, percentChange), spatialElement, bigChangesRooms);
                }
            }

            transaction.Commit();
        }

        Warnings.AddRange(bigChangesRooms.Values);
        if(!_errorWindowService.ShowNoticeWindow(NotShowWarnings, Warnings)) {
            string message = _localizationService.GetLocalizedString("TaskDialog.Result");
            string title = _localizationService.GetLocalizedString("TaskDialog.Information");
            _messageBoxService.Show(message, title);
        }
    }

    private bool CanCalculateAreas(object p) {
        return Levels.Any(item => item.IsSelected);
    }

    private void Calculate(object p) {
        // Удаляем все не размещенные помещения
        _revitRepository.RemoveUnplacedSpatialElements();

        // Получаем список дополнительных стадий
        var phases = AdditionalPhases.ToList();
        phases.Add(Phase);

        // Получение всех помещений
        // по заданной стадии
        var levels = Levels.Where(item => item.IsSelected);

        // Проверка всех элементов
        // на выделенных уровнях
        if(CheckElements(phases, levels)) {
            _errorWindowService.ShowNoticeWindow(NotShowWarnings, Warnings);
            return;
        }

        // Расчет площадей помещений
        CalculateAreas(phases, levels);

        // Сохранение
        // текущей конфигурации
        SaveRoomsConfig();
    }

    private bool CanCalculate(object p) {
        if(IsCheckRoomsChanges) {
            if(!int.TryParse(RoomAccuracy, out int checkRoomAccuracy)) {
                ErrorText = _localizationService.GetLocalizedString("RoomsWindow.WarningAccuracy");
                return false;
            }

            if(checkRoomAccuracy <= 0 || checkRoomAccuracy > 100) {
                ErrorText = _localizationService.GetLocalizedString("RoomsWindow.WarningAccuracyRange");
                return false;
            }
        }

        if(Phase == null) {
            ErrorText = _localizationService.GetLocalizedString("RoomsWindow.WarningSelectPhase");
            return false;
        }

        if(!Levels.Any(item => item.IsSelected)) {
            ErrorText = _localizationService.GetLocalizedString("RoomsWindow.WarningSelectLevels");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private bool CheckElements(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
        var errorElements = new Dictionary<string, WarningViewModel>();
        foreach(var level in levels) {
            var rooms = level.GetRooms(phases);

            // Все помещения которые
            // избыточные или не окруженные
            var redundantRooms = rooms.Where(item => item.IsRedundant == true || item.NotEnclosed == true);
            AddElements(WarningInfo.GetRedundantRooms(_localizationService), redundantRooms, errorElements);

            // Все помещения у которых
            // не заполнены обязательные параметры
            foreach(var room in rooms) {
                if(room.Room == null) {
                    AddElement(WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomName.Name),
                        null, room, errorElements);
                }

                if(room.RoomGroup == null) {
                    AddElement(
                        WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomGroupName.Name),
                        null, room, errorElements);
                }

                if(room.RoomSection == null) {
                    AddElement(
                        WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomSectionName.Name),
                        null, room, errorElements);
                }
            }

            // Все помещения у которых
            // не совпадают значения группы и типа группы
            var checksRooms = rooms.Where(room => room.RoomGroup != null && room.RoomSection != null)
                .Where(room => room.Phase == Phase || room.PhaseName.Equals("Межквартирные перегородки",
                    StringComparison.CurrentCultureIgnoreCase))
                .Where(ContainGroups);

            var flats = checksRooms
                .GroupBy(item => new { s = item.RoomSection.Id, g = item.RoomGroup.Id, item.LevelId });

            foreach(var flat in flats) {
                if(IsNotEqualGroupType(flat)) {
                    string roomGroup = flat.FirstOrDefault()?.RoomGroup.Name;
                    string roomSection = flat.FirstOrDefault()?.RoomSection.Name;
                    AddElements(WarningInfo.GetNotEqualGroupType(_localizationService, roomGroup, roomSection), flat,
                        errorElements);
                }

                if(IsNotEqualMultiLevel(flat.Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup)))) {
                    AddElements(WarningInfo.GetNotEqualMultiLevel(_localizationService), flat, errorElements);
                }
            }
        }

        // Ошибки, которые не останавливают выполнение скрипта
        var warnings = new Dictionary<string, WarningViewModel>();

        var checkPhases = new List<PhaseViewModel>() { Phase };
        var customPhase = phases.FirstOrDefault(item =>
            item.PhaseName?.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase) == true);
        if(customPhase != null) {
            checkPhases.Add(customPhase);
        }

        foreach(var level in levels) {
            var rooms = level.GetRooms(checkPhases).ToArray();

            CheckRoomSeparators(level, checkPhases, warnings, rooms);
            CheckDoorsAndWindows(level, checkPhases, warnings, rooms);

            // Все помещений у которых
            // найдены самопересечения
            var contourIntersectRooms = rooms
                .Where(item => item.IsCountourIntersect == true);
            AddElements(WarningInfo.GetContourIntersectRooms(_localizationService), contourIntersectRooms, warnings);
        }

        Warnings = warnings.Values.Union(errorElements.Values).ToList();
        return errorElements.Count > 0;
    }

    private void CheckRoomSeparators(
        LevelViewModel level,
        List<PhaseViewModel> checkPhases,
        Dictionary<string, WarningViewModel> warningElements,
        SpatialElementViewModel[] rooms) {
        // добавляем разделители помещений
        var separators = level.GetRoomSeparators(checkPhases).ToArray();
        foreach(var roomSeparator in separators) {
            roomSeparator.AddRooms(rooms);
        }

        // Все разделители
        // с не совпадающей секцией
        var notEqualSectionDoors = separators
            .Where(item => !item.IsSectionNameEqual);

        AddElements(WarningInfo.GetEqualSectionDoors(_localizationService), notEqualSectionDoors, warningElements);

        // Все разделители
        // с не совпадающей группой
        var notEqualGroup = separators
            .Where(item => !item.IsGroupNameEqual);

        AddElements(WarningInfo.GetNotEqualGroup(_localizationService), notEqualGroup, warningElements);
    }

    private void CheckDoorsAndWindows(
        LevelViewModel level,
        List<PhaseViewModel> checkPhases,
        Dictionary<string, WarningViewModel> warningElements,
        SpatialElementViewModel[] rooms) {
        var doors = level.GetDoors(checkPhases).ToArray();
        var doorsAndWindows = doors.Union(level.GetWindows(checkPhases)).ToArray();

        // Все двери
        // с не совпадающей секцией
        var notEqualSectionDoors = doorsAndWindows
            .Where(item => !item.IsSectionNameEqual);

        AddElements(WarningInfo.GetEqualSectionDoors(_localizationService), notEqualSectionDoors, warningElements);

        // Все окна и двери
        // с не совпадающей группой
        var notEqualGroup = doorsAndWindows
            .Where(item => !item.IsGroupNameEqual);

        AddElements(WarningInfo.GetNotEqualGroup(_localizationService), notEqualGroup, warningElements);
    }

    private void CalculateAreas(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
        // получаем обработанные имена уровней
        var levelNames = _revitRepository.GetLevelNames();
        var bigChangesRooms = new Dictionary<string, WarningViewModel>();

        string transactionName = _localizationService.GetLocalizedString("Transaction.CalculateAreas");
        using(var transaction = _revitRepository.Document.StartTransaction(transactionName)) {
            // Надеюсь будет достаточно быстро отрабатывать :)
            // Подсчет площадей помещений
            foreach(var level in levels) {
                foreach(var spatialElement in level.GetRooms(phases)) {
                    if(IsFillLevel && !spatialElement.IsLevelFix) {
                        // Заполняем параметр Этаж
                        _revitRepository.UpdateLevelSharedParam(spatialElement.Element, levelNames);
                    }

                    // Заполняем дублирующие
                    // общие параметры
                    spatialElement.UpdateSharedParams();

                    // Обновление параметра площади 
                    var area = new RoomAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                    area.CalculateParam(spatialElement);
                    bool isChangedRoomArea = area.SetParamValue(spatialElement);

                    // Площадь с коэффициентном зависит от площади без коэффициента
                    var areaWithRatio = new AreaWithRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                    areaWithRatio.CalculateParam(spatialElement);
                    areaWithRatio.SetParamValue(spatialElement);

                    if(isChangedRoomArea && IsCheckRoomsChanges) {
                        double differences = areaWithRatio.GetDifferences();
                        double percentChange = areaWithRatio.GetPercentChange();
                        AddElement(WarningInfo.GetBigChangesRoomAreas(_localizationService), FormatMessage(differences, percentChange),
                            spatialElement, bigChangesRooms);
                    }
                }
            }

            // Обработка параметров зависящих от квартир
            var flats = levels
                .SelectMany(item => item.GetRooms(phases))
                .Where(item => string.IsNullOrEmpty(item.RoomMultilevelGroup))
                .GroupBy(item => new { s = item.RoomSection.Id, g = item.RoomGroup.Id, item.LevelId });

            foreach(var flat in flats) {
                UpdateParam(flat.ToArray(), bigChangesRooms);
            }

            // многоуровневые квартиры
            var multiLevels = levels
                .SelectMany(item => item.GetRooms(phases))
                .Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup))
                .GroupBy(item => new { item.RoomSection.Id, item.RoomMultilevelGroup });

            foreach(var multiLevel in multiLevels) {
                UpdateParam(multiLevel.ToArray(), bigChangesRooms);
            }

            transaction.Commit();
        }

        Warnings.AddRange(bigChangesRooms.Values);
        if(!_errorWindowService.ShowNoticeWindow(NotShowWarnings, Warnings)) {
            string message = _localizationService.GetLocalizedString("TaskDialog.Result");
            string title = _localizationService.GetLocalizedString("TaskDialog.Information");
            _messageBoxService.Show(message, title);
        }
    }

    private void UpdateParam(SpatialElementViewModel[] flat, Dictionary<string, WarningViewModel> bigChangesRooms) {
        foreach(var calculation in GetParamCalculations()) {
            foreach(var room in flat) {
                calculation.CalculateParam(room);
            }

            foreach(var room in flat) {
                if(calculation.SetParamValue(room) && IsCheckRoomsChanges &&
                   calculation.RevitParam == SharedParamsConfig.Instance.ApartmentArea) {
                    double differences = calculation.GetDifferences();
                    double percentChange = calculation.GetPercentChange();
                    AddElement(WarningInfo.GetBigChangesFlatAreas(_localizationService), 
                               FormatMessage(differences, percentChange), 
                               room,
                               bigChangesRooms);
                }
            }
        }
    }

    private string FormatMessage(double differences, double percentChange) {
        return _localizationService.GetLocalizedString(
            "WarningsWindow.BigChangeDiff", $"{percentChange:F0}", $"{differences:F}", GetSquareMetersText());
    }

    private IEnumerable<IEnumerable<SpatialElementViewModel>> GetFlats(IEnumerable<SpatialElementViewModel> rooms) {
        foreach(var section in rooms.GroupBy(item => item.RoomSection.Id)) {
            foreach(var flat in section.GroupBy(item => item.RoomGroup.Id)) {
                yield return flat;
            }
        }
    }

    private IEnumerable<IParamCalculation> GetParamCalculations() {
        yield return new ApartmentAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
        yield return new ApartmentAreaRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
        yield return new ApartmentLivingAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
        yield return new ApartmentAreaNoBalconyCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };

        if(IsCountRooms) {
            yield return new RoomsCountCalculation() { Phase = Phase.Element };
        }

        if(IsSpotCalcArea) {
            yield return new ApartmentFullAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
        }
    }

    private IEnumerable<SpatialElementViewModel> GetAreas() {
        return Levels.Where(item => item.IsSelected).SelectMany(item => item.GetAreas());
    }

    private static bool IsNotEqualGroupType(IEnumerable<SpatialElementViewModel> rooms) {
        return rooms
            .Select(group => group.RoomTypeGroup?.Id)
            .Distinct().Count() > 1;
    }

    private static bool IsNotEqualMultiLevel(IEnumerable<SpatialElementViewModel> rooms) {
        return rooms
            .Select(group => group.RoomMultilevelGroup)
            .Distinct().Count() > 1;
    }

    private static bool ContainGroups(SpatialElementViewModel item) {
        return new[] { "апартаменты", "квартира", "гостиничный номер", "пентхаус" }
            .Any(group => Contains(item.RoomGroup.Name, group, StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool Contains(string source, string toCheck, StringComparison comp) {
        return source?.IndexOf(toCheck, comp) >= 0;
    }

    private int GetRoomAccuracy() {
        return int.TryParse(RoomAccuracy, out int result) ? result : 100;
    }

    private void AddElements(WarningInfo infoElement, 
                             IEnumerable<IElementViewModel<Element>> elements, 
                             Dictionary<string, WarningViewModel> infoElements) {
        foreach(var element in elements) {
            AddElement(infoElement, null, element, infoElements);
        }
    }

    private void AddElement(WarningInfo infoElement, 
                            string message, 
                            IElementViewModel<Element> element, 
                            Dictionary<string, 
                            WarningViewModel> infoElements) {
        if(!infoElements.TryGetValue(infoElement.Message, out var value)) {
            value = new WarningViewModel(_localizationService) { 
                Message = infoElement.Message, 
                TypeInfo = infoElement.TypeInfo, 
                Description = infoElement.Description, 
                Elements = [] };
            infoElements.Add(infoElement.Message, value);
        }

        value.Elements.Add(new WarningElementViewModel() { Element = element, Description = message });
    }

    //private bool ShowInfoElementsWindow(string title, IEnumerable<InfoElementViewModel> infoElements) {
    //    if(NotShowWarnings) {
    //        infoElements = infoElements.Where(item => item.TypeInfo != TypeInfo.Warning);
    //    }

    //    if(infoElements.Any()) {
    //        var window = new InfoElementsWindow() {
    //            Title = title,
    //            DataContext = new InfoElementsViewModel() {
    //                InfoElement = infoElements.FirstOrDefault(),
    //                InfoElements = [.. infoElements]
    //            }
    //        };

    //        window.Show();
    //        return true;
    //    }

    //    return false;
    //}

    protected IEnumerable<SpatialElement> GetAdditionalElements(IList<SpatialElement> selectedElements) {
        var levelIds = selectedElements.Select(item => item.LevelId).Distinct().ToArray();
        var additionalPhases = _revitRepository.GetAdditionalPhases().Select(item => item.Id).ToArray();
        return _revitRepository.GetSpatialElements()
            .Where(item => levelIds.Contains(item.LevelId))
            .Where(item => additionalPhases.Contains(_revitRepository.GetPhaseId(item)));
    }

#if REVIT_2020_OR_LESS
    private string GetSquareMetersText() {
        return LabelUtils.GetLabelFor(DisplayUnitType.DUT_SQUARE_METERS);
    }
#else
    private string GetSquareMetersText() {
        return LabelUtils.GetLabelForUnit(UnitTypeId.SquareMeters);
    }
#endif
}
