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
        protected readonly RevitRepository _revitRepository;
        private string _errorText;
        private bool _isCheckedSelected;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
            AdditionalPhases = new ObservableCollection<PhaseViewModel>(_revitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, _revitRepository)));

            Phases = new ObservableCollection<PhaseViewModel>(Levels.SelectMany(item => item.SpartialElements).Select(item => item.Phase).Distinct().Except(AdditionalPhases));
            Phase = Phases.FirstOrDefault();

            RoundAccuracy = 1;
            RoundAccuracyValues = new ObservableCollection<int>(Enumerable.Range(1, 3));

            CalculateCommand = new RelayCommand(Calculate, CanCalculate);

        }

        public ICommand CalculateCommand { get; }

        public bool IsCheckedSelected {
            get => _isCheckedSelected;
            set {
                this.RaiseAndSetIfChanged(ref _isCheckedSelected, value);
                foreach(var level in Levels) {
                    level.IsSelected = _isCheckedSelected;
                }
            }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string Name { get; set; }
        public PhaseViewModel Phase { get; set; }

        public bool IsSpotCalcArea { get; set; }
        public bool IsCheckRoomsChanges { get; set; }
        public string CheckRoomAccuracy { get; set; } = "100";

        public int RoundAccuracy { get; set; }
        public ObservableCollection<int> RoundAccuracyValues { get; }

        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<LevelViewModel> Levels { get; }
        public ObservableCollection<PhaseViewModel> AdditionalPhases { get; }

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();
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
                return;
            }

            // Расчет площадей помещений
            CalculateAreas(phases, levels);
        }

        private bool CanCalculate(object p) {
            if(IsCheckRoomsChanges) {
                if(!int.TryParse(CheckRoomAccuracy, out var сheckRoomAccuracy)) {
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
                var rooms = level.GetSpatialElementViewModels(phases);

                // Все помещения которые
                // избыточные или не окруженные
                var redundantRooms = rooms.Where(item => item.IsRedundant == true || item.NotEnclosed == true);
                AddElements("Избыточные или не окруженные.", TypeInfo.Error, redundantRooms, errorElements);

                // Все помещения у которых
                // не заполнены обязательные параметры
                foreach(var room in rooms) {
                    if(room.Room == null) {
                        AddElement($"Не заполнен обязательный параметр \"{ProjectParamsConfig.Instance.RoomName.Name}\".", TypeInfo.Error, room, errorElements);
                    }

                    if(room.RoomGroup == null) {
                        AddElement($"Не заполнен обязательный параметр \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\".", TypeInfo.Error, room, errorElements);
                    }

                    if(room.RoomSection == null) {
                        AddElement($"Не заполнен обязательный параметр \"{ProjectParamsConfig.Instance.RoomSectionName.Name}\".", TypeInfo.Error, room, errorElements);
                    }
                }

                // Все помещения у которых
                // не совпадают значения группы и типа группы
                var notEqualGroupTypeRooms = rooms.Where(room => room.RoomGroup != null)
                    .Where(room => room.Phase == Phase || room.PhaseName.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase))
                    .Where(room => ContainGroups(room))
                    .GroupBy(room => room.RoomGroup.Id)
                    .Where(group => IsGroupTypeEqual(group))
                    .SelectMany(items => items);

                AddElements("Не совпадают значения параметров групп и типа групп параметра.", TypeInfo.Error, notEqualGroupTypeRooms, errorElements);
            }

            // Обрабатываем все зоны
            var redundantAreas = GetAreas().Where(item => item.IsRedundant == true || item.NotEnclosed == true);
            AddElements("Избыточные или не окруженные.", TypeInfo.Error, redundantAreas, errorElements);

            // Ошибки, которые не останавливают выполнение скрипта
            var warningElements = new Dictionary<string, InfoElementViewModel>();
            foreach(var level in levels) {
                var doors = level.GetDoors(phases);
                var rooms = level.GetSpatialElementViewModels(phases);

                // Все двери
                // с не совпадающей секцией
                var notEqualSectionDoors = doors.Where(room => room.Phase == Phase || room.PhaseName.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase)).Where(item => !item.IsSectionNameEqual);
                AddElements("Не совпадают секции.", TypeInfo.Warning, notEqualSectionDoors, warningElements);

                // Все помещений у которых
                // найдены самопересечения
                var countourIntersectRooms = rooms.Where(item => item.IsCountourIntersect == true);
                AddElements("Найдены самопересечения.", TypeInfo.Warning, countourIntersectRooms, warningElements);
            }

            ShowInfoElementsWindow("Ошибки", warningElements.Values.Union(errorElements.Values));
            return errorElements.Count > 0;
        }

        private void CalculateAreas(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
            using(var transaction = _revitRepository.StartTransaction("Расчет площадей")) {
                // Надеюсь будет достаточно быстро отрабатывать :)
                // Подсчет площадей помещений

                var bigChangesRooms = new Dictionary<string, InfoElementViewModel>();

                // Обновление параметра округления у зон
                foreach(var spartialElement in GetAreas().Where(item => item.Phase == Phase)) {
                    // Обновление параметра
                    // площади с коэффициентом
                    var areaWithRatio = new AreaWithRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                    if(areaWithRatio.SetParamValue(spartialElement)) {
                        AddElement("Большие изменения в площади.", TypeInfo.Info, spartialElement, bigChangesRooms);
                    }
                }

                foreach(var level in levels) {
                    foreach(var spartialElement in level.GetSpatialElementViewModels(phases)) {
                        // Заполняем дублирующие
                        // общие параметры
                        spartialElement.UpdateSharedParams();

                        // Обновляем общий параметр этажа
                        spartialElement.UpdateLevelSharedParam();

                        // Обновление параметра
                        // площади с коэффициентом
                        var areaWithRatio = new AreaWithRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                        if(areaWithRatio.SetParamValue(spartialElement)) {
                            AddElement("Большие изменения в площади.", TypeInfo.Info, spartialElement, bigChangesRooms);
                        }
                    }
                }

                // Обработка параметров зависящих от квартир
                foreach(var level in levels) {
                    var rooms = level.GetSpatialElementViewModels(phases).ToList();
                    foreach(var section in rooms.GroupBy(item => item.RoomSection.Name)) {
                        foreach(var flat in section.GroupBy(item => item.RoomGroup.Name)) {
                            var roomsCount = new RoomsCountCalculation() { Phase = Phase.Element };
                            var apartmentArea = new ApartmentAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                            var apartmentFullArea = new ApartmentFullAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                            var apartmentAreaRatio = new ApartmentAreaRatioCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                            var apartmentLivingArea = new ApartmentLivingAreaCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };
                            var apartmentAreaNoBalcony = new ApartmentAreaNoBalconyCalculation(GetRoomAccuracy(), RoundAccuracy) { Phase = Phase.Element };

                            // Расчет параметров
                            foreach(var room in flat) {
                                apartmentArea.CalculateParam(room);
                                apartmentAreaRatio.CalculateParam(room);
                                apartmentLivingArea.CalculateParam(room);
                             
                                if(IsSpotCalcArea) {
                                    apartmentFullArea.CalculateParam(room);
                                }

                                roomsCount.CalculateParam(room);
                            }

                            // Применение параметров
                            foreach(var room in flat) {
                                if(IsSpotCalcArea) {
                                    apartmentFullArea.SetParamValue(room);
                                }

                                if(apartmentArea.SetParamValue(room)) {
                                    AddElement("Большие изменения в площади комнат.", TypeInfo.Info, room, bigChangesRooms);
                                }

                                roomsCount.SetParamValue(room);
                                apartmentAreaRatio.SetParamValue(room);
                                apartmentLivingArea.SetParamValue(room);
                                apartmentAreaNoBalcony.SetParamValue(room);
                            }
                        }
                    }
                }

                transaction.Commit();
                ShowInfoElementsWindow("Значительные изменения площадей", bigChangesRooms.Values);
            }
        }

        private IEnumerable<SpatialElementViewModel> GetAreas() {
            return _revitRepository.GetAllAreas()
                .Select(item => new SpatialElementViewModel(item, _revitRepository));
        }

        private bool GetIsBigChanges(double areaOldValue, double areaNewValue) {
            if(IsCheckRoomsChanges) {
                return Math.Abs(areaOldValue - areaNewValue) / areaOldValue * 100 > GetRoomAccuracy();
            }

            return false;
        }

        private static bool IsGroupTypeEqual(IEnumerable<SpatialElementViewModel> rooms) {
            return rooms
                .Select(group => group.RoomTypeGroup.Name)
                .Distinct(StringComparer.CurrentCultureIgnoreCase).Count() > 1;
        }

        private static bool ContainGroups(SpatialElementViewModel item) {
            return new[] { "апартаменты", "квартира", "гостиничный номер", "пентхаус" }
                .Any(group => Contains(item.RoomGroup.Name, group, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool Contains(string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        private int GetRoomAccuracy() {
            return int.TryParse(CheckRoomAccuracy, out int result) ? result : 100;
        }

#if D2020 || R2020
        private double ConvertValueToSquareMeters(double? value) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS), RoundAccuracy) : 0;
        }

        private double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
        }
#elif D2021 || R2021 || D2022 || R2022
        private double ConvertValueToSquareMeters(double? value) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters), RoundAccuracy) : 0;
        }

        private double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.SquareMeters);
        }
#endif

        private void AddElements(string infoText, TypeInfo typeInfo, IEnumerable<IElementViewModel<Element>> elements, Dictionary<string, InfoElementViewModel> infoElements) {
            foreach(var element in elements) {
                AddElement(infoText, typeInfo, element, infoElements);
            }
        }

        private void AddElement(string infoText, TypeInfo typeInfo, IElementViewModel<Element> element, Dictionary<string, InfoElementViewModel> infoElements) {
            if(!infoElements.TryGetValue(infoText, out var value)) {
                value = new InfoElementViewModel() { Message = infoText, TypeInfo = typeInfo, Elements = new ObservableCollection<IElementViewModel<Element>>() };
                infoElements.Add(infoText, value);
            }

            value.Elements.Add(element);
        }

        private void ShowInfoElementsWindow(string title, IEnumerable<InfoElementViewModel> infoElements) {
            if(infoElements.Any()) {
                var window = new InfoElementsWindow() {
                    Title = title,
                    DataContext = new InfoElementsViewModel() {
                        InfoElements = new ObservableCollection<InfoElementViewModel>(infoElements)
                    }
                };

                window.Show();
            }
        }
    }
}
