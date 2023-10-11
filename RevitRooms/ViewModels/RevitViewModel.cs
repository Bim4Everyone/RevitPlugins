using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.Models.Calculation;
using RevitRooms.Views;

namespace RevitRooms.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        public Guid _id;
        protected readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _isAllowSelectLevels;
        private bool _isFillLevel;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels().OrderBy(item => item.Element.Elevation).Where(item => item.SpatialElements.Count > 0));
            AdditionalPhases = new ObservableCollection<PhaseViewModel>(_revitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, _revitRepository)));

            Phases = new ObservableCollection<PhaseViewModel>(Levels.SelectMany(item => item.SpatialElements).Select(item => item.Phase).Where(item => item != null).Distinct().Except(AdditionalPhases));
            Phase = Phases.FirstOrDefault();

            RoundAccuracy = 1;
            RoundAccuracyValues = new ObservableCollection<int>(Enumerable.Range(1, 3));

            CalculateCommand = new RelayCommand(Calculate, CanCalculate);
            CalculateAreasCommand = new RelayCommand(CalculateAreas, CanCalculateAreas);

            // Установка конфигурации
            SetRoomsConfig();
        }

        public ICommand CalculateCommand { get; }
        public ICommand CalculateAreasCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool IsAllowSelectLevels {
            get => _isAllowSelectLevels;
            set => this.RaiseAndSetIfChanged(ref _isAllowSelectLevels, value);
        }
        
        public bool IsFillLevel {
            get => _isFillLevel;
            set => this.RaiseAndSetIfChanged(ref _isFillLevel, value);
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

        private List<InfoElementViewModel> InfoElements { get; set; } = new List<InfoElementViewModel>();

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();

        private void SetRoomsConfig() {
            var roomsConfig = RoomsConfig.GetRoomsConfig();
            var settings = roomsConfig.GetSettings(_revitRepository.Document);
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
            var roomsConfig = RoomsConfig.GetRoomsConfig();
            var settings = roomsConfig.GetSettings(_revitRepository.Document);
            if(settings == null) {
                settings = roomsConfig.AddSettings(_revitRepository.Document);
            }

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

            roomsConfig.SaveProjectConfig();
        }

        private void CalculateAreas(object p) {
            // Удаляем все не размещенные помещения
            _revitRepository.RemoveUnplacedSpatialElements();

            // Обрабатываем все зоны
            var errorElements = new Dictionary<string, InfoElementViewModel>();
            var redundantAreas = GetAreas().Where(item => item.IsRedundant == true || item.NotEnclosed == true);
            AddElements(InfoElement.RedundantAreas, redundantAreas, errorElements);

            InfoElements = errorElements.Values.ToList();
            if(InfoElements.Count > 0) {
                ShowInfoElementsWindow("Ошибки", InfoElements);
                return;
            }

            // получаем уже обработанные имена уровней
            Dictionary<ElementId, string> levelNames = _revitRepository.GetLevelNames();
            
            var bigChangesRooms = new Dictionary<string, InfoElementViewModel>();
            using(var transaction = _revitRepository.StartTransaction("Расчет площадей")) {
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
                        var differences = areaWithRatio.GetDifferences();
                        var percentChange = areaWithRatio.GetPercentChange();
                        AddElement(InfoElement.BigChangesAreas, FormatMessage(differences, percentChange), spatialElement, bigChangesRooms);
                    }
                }

                transaction.Commit();
            }
            
            InfoElements.AddRange(bigChangesRooms.Values);
            if(!ShowInfoElementsWindow("Информация", InfoElements)) {
                TaskDialog.Show("Предупреждение!", "Расчет завершен!");
            }
        }
        
        private bool CanCalculateAreas(object p) {
            if(!Levels.Any(item => item.IsSelected)) {
                return false;
            }

            return true;
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
                ShowInfoElementsWindow("Ошибки", InfoElements);
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
                if(!int.TryParse(RoomAccuracy, out var сheckRoomAccuracy)) {
                    ErrorText = "Точность проверки должна быть числом.";
                    return false;
                }

                if(сheckRoomAccuracy <= 0 || сheckRoomAccuracy > 100) {
                    ErrorText = "Точность проверки должна быть от 1 до 100.";
                    return false;
                }
            }

            if(Phase == null) {
                ErrorText = "Выберите стадию.";
                return false;
            }

            if(!Levels.Any(item => item.IsSelected)) {
                ErrorText = "Выберите хотя бы один уровень.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private bool CheckElements(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
            var errorElements = new Dictionary<string, InfoElementViewModel>();
            foreach(var level in levels) {
                var rooms = level.GetRooms(phases);

                // Все помещения которые
                // избыточные или не окруженные
                var redundantRooms = rooms.Where(item => item.IsRedundant == true || item.NotEnclosed == true);
                AddElements(InfoElement.RedundantRooms, redundantRooms, errorElements);

                // Все помещения у которых
                // не заполнены обязательные параметры
                foreach(var room in rooms) {
                    if(room.Room == null) {
                        AddElement(InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomName.Name),
                            null, room, errorElements);
                    }

                    if(room.RoomGroup == null) {
                        AddElement(
                            InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomGroupName.Name),
                            null, room, errorElements);
                    }

                    if(room.RoomSection == null) {
                        AddElement(
                            InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomSectionName.Name),
                            null, room, errorElements);
                    }
                }

                // Все помещения у которых
                // не совпадают значения группы и типа группы
                var checksRooms = rooms.Where(room => room.RoomGroup != null && room.RoomSection != null)
                    .Where(room => room.Phase == Phase || room.PhaseName.Equals("Межквартирные перегородки",
                        StringComparison.CurrentCultureIgnoreCase))
                    .Where(room => ContainGroups(room));

                var flats = checksRooms
                    .GroupBy(item => new {s = item.RoomSection.Id, g = item.RoomGroup.Id, item.LevelId});

                foreach(var flat in flats) {
                    if(IsNotEqualGroupType(flat)) {
                        var roomGroup = flat.FirstOrDefault()?.RoomGroup.Name;
                        var roomSection = flat.FirstOrDefault()?.RoomSection.Name;
                        AddElements(InfoElement.NotEqualGroupType.FormatMessage(roomGroup, roomSection), flat,
                            errorElements);
                    }

                    if(IsNotEqualMultiLevel(flat.Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup)))) {
                        AddElements(InfoElement.NotEqualMultiLevel, flat, errorElements);
                    }
                }
            }

            // Ошибки, которые не останавливают выполнение скрипта
            var warningElements = new Dictionary<string, InfoElementViewModel>();

            var checkPhases = new List<PhaseViewModel>() {Phase};
            var customPhase = phases.FirstOrDefault(item =>
                item.PhaseName?.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase) == true);
            if(customPhase != null) {
                checkPhases.Add(customPhase);
            }

            foreach(var level in levels) {
                var rooms = level.GetRooms(checkPhases).ToArray();
                
                CheckRoomSeparators(level, checkPhases, warningElements, rooms);
                CheckDoorsAndWindows(level, checkPhases, warningElements, rooms);
                
                // Все помещений у которых
                // найдены самопересечения
                var countourIntersectRooms = rooms
                    .Where(item => item.IsCountourIntersect == true);
                AddElements(InfoElement.CountourIntersectRooms, countourIntersectRooms, warningElements);
            }

            InfoElements = warningElements.Values.Union(errorElements.Values).ToList();
            return errorElements.Count > 0;
        }

        private void CheckRoomSeparators(
            LevelViewModel level,
            List<PhaseViewModel> checkPhases,
            Dictionary<string, InfoElementViewModel> warningElements, 
            SpatialElementViewModel[] rooms) {
            // добавляем разделители помещений
            var separators = level.GetRoomSeparators(checkPhases).ToArray();
            foreach(RoomSeparatorViewModel roomSeparator in separators) {
                roomSeparator.AddRooms(rooms);
            }
            
            // Все разделители
            // с не совпадающей секцией
            var notEqualSectionDoors = separators
                .Where(item => !item.IsSectionNameEqual);

            AddElements(InfoElement.NotEqualSectionDoors, notEqualSectionDoors, warningElements);

            // Все разделители
            // с не совпадающей группой
            var notEqualGroup = separators
                .Where(item => !item.IsGroupNameEqual);

            AddElements(InfoElement.NotEqualGroup, notEqualGroup, warningElements);
        }

        private void CheckDoorsAndWindows(
            LevelViewModel level,
            List<PhaseViewModel> checkPhases,
            Dictionary<string, InfoElementViewModel> warningElements,
            SpatialElementViewModel[] rooms) {
            var doors = level.GetDoors(checkPhases).ToArray();
            var doorsAndWindows = doors.Union(level.GetWindows(checkPhases)).ToArray();

            // Все двери
            // с не совпадающей секцией
            var notEqualSectionDoors = doorsAndWindows
                .Where(item => !item.IsSectionNameEqual);

            AddElements(InfoElement.NotEqualSectionDoors, notEqualSectionDoors, warningElements);

            // Все окна и двери
            // с не совпадающей группой
            var notEqualGroup = doorsAndWindows
                .Where(item => !item.IsGroupNameEqual);

            AddElements(InfoElement.NotEqualGroup, notEqualGroup, warningElements);
        }

        private void CalculateAreas(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
            using(var transaction = _revitRepository.StartTransaction("Расчет площадей")) {
                // получаем обработанные имена уровней
                Dictionary<ElementId, string> levelNames = _revitRepository.GetLevelNames();
               
                var bigChangesRooms = new Dictionary<string, InfoElementViewModel>();

                // Надеюсь будет достаточно быстро отрабатывать :)
                // Подсчет площадей помещений
                foreach(var level in levels) {
                    foreach(var spatialElement in level.GetRooms(phases)) {
                        if(IsFillLevel) {
                            // Заполняем параметр Этаж
                            _revitRepository.UpdateLevelSharedParam(spatialElement.Element, levelNames);
                        }
                        
                        // Заполняем дублирующие
                        // общие параметры
                        spatialElement.UpdateSharedParams();

                        // Обновление параметра площади 
                        var area = new RoomAreaCalculation(GetRoomAccuracy(), RoundAccuracy) {Phase = Phase.Element};
                        area.CalculateParam(spatialElement);
                        bool isChangedRoomArea = area.SetParamValue(spatialElement);

                        // Площадь с коэффициентном зависит от площади без коэффициента
                        var areaWithRatio = new AreaWithRatioCalculation(GetRoomAccuracy(), RoundAccuracy) {Phase = Phase.Element};
                        areaWithRatio.CalculateParam(spatialElement);
                        areaWithRatio.SetParamValue(spatialElement);

                        if(isChangedRoomArea && IsCheckRoomsChanges) {
                            var differences = areaWithRatio.GetDifferences();
                            var percentChange = areaWithRatio.GetPercentChange();
                            AddElement(InfoElement.BigChangesRoomAreas, FormatMessage(differences, percentChange),
                                spatialElement, bigChangesRooms);
                        }
                    }
                }

                // Обработка параметров зависящих от квартир
                var flats = levels
                    .SelectMany(item => item.GetRooms(phases))
                    .Where(item => string.IsNullOrEmpty(item.RoomMultilevelGroup))
                    .GroupBy(item=> new {s = item.RoomSection.Id, g = item.RoomGroup.Id, item.LevelId});

                foreach(var flat in flats) {
                    UpdateParam(flat.ToArray(), bigChangesRooms);
                }

                // многоуровневые квартиры
                var multiLevels = levels
                    .SelectMany(item => item.GetRooms(phases))
                    .Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup))
                    .GroupBy(item=> new {item.RoomSection.Id, item.RoomMultilevelGroup});
                
                foreach(var multiLevel in multiLevels) {
                    UpdateParam(multiLevel.ToArray(), bigChangesRooms);
                }


                transaction.Commit();
                InfoElements.AddRange(bigChangesRooms.Values);
                if(!ShowInfoElementsWindow("Информация", InfoElements)) {
                    TaskDialog.Show("Предупреждение!", "Расчет завершен!");
                }
            }
        }

        private void UpdateParam(SpatialElementViewModel[] flat, Dictionary<string, InfoElementViewModel> bigChangesRooms) {
            foreach(var calculation in GetParamCalculations()) {
                foreach(var room in flat) {
                    calculation.CalculateParam(room);
                }

                foreach(var room in flat) {
                    if(calculation.SetParamValue(room) && IsCheckRoomsChanges &&
                       calculation.RevitParam == SharedParamsConfig.Instance.ApartmentArea) {
                        var differences = calculation.GetDifferences();
                        var percentChange = calculation.GetPercentChange();
                        AddElement(InfoElement.BigChangesFlatAreas, FormatMessage(differences, percentChange), room,
                            bigChangesRooms);
                    }
                }
            }
        }

        private string FormatMessage(double differences, double percentChange) {
            return $"Изменение: {percentChange:F0}% ({differences:F} {GetSquareMetersText()}).";
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

        private void AddElements(InfoElement infoElement, IEnumerable<IElementViewModel<Element>> elements, Dictionary<string, InfoElementViewModel> infoElements) {
            foreach(var element in elements) {
                AddElement(infoElement, null, element, infoElements);
            }
        }

        private void AddElement(InfoElement infoElement, string message, IElementViewModel<Element> element, Dictionary<string, InfoElementViewModel> infoElements) {
            if(!infoElements.TryGetValue(infoElement.Message, out var value)) {
                value = new InfoElementViewModel() { Message = infoElement.Message, TypeInfo = infoElement.TypeInfo, Description = infoElement.Description, Elements = new ObservableCollection<MessageElementViewModel>() };
                infoElements.Add(infoElement.Message, value);
            }

            value.Elements.Add(new MessageElementViewModel() { Element = element, Description = message });
        }

        private bool ShowInfoElementsWindow(string title, IEnumerable<InfoElementViewModel> infoElements) {
            if(NotShowWarnings) {
                infoElements = infoElements.Where(item => item.TypeInfo != TypeInfo.Warning);
            }

            if(infoElements.Any()) {
                var window = new InfoElementsWindow() {
                    Title = title,
                    DataContext = new InfoElementsViewModel() {
                        InfoElement = infoElements.FirstOrDefault(),
                        InfoElements = new ObservableCollection<InfoElementViewModel>(infoElements)
                    }
                };

                window.Show();
                return true;
            }

            return false;
        }

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
}
