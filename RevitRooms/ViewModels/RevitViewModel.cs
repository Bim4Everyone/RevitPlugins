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
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;
        private string _errorText;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
            Phases = new ObservableCollection<PhaseViewModel>(Levels.SelectMany(item => item.Rooms).Select(item => item.Phase).Distinct());
            Phase = Phases.FirstOrDefault();

            RoundAccuracy = 1;
            RoundAccuracyValues = new ObservableCollection<int>(Enumerable.Range(1, 3));

            CalculateCommand = new RelayCommand(Calculate, CanCalculate);
        }

        public ICommand CalculateCommand { get; }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string Name { get; set; }
        public PhaseViewModel Phase { get; set; }

        public bool IsSpotCalcArea { get; set; }
        public bool IsCheckRoomsChanges { get; set; }
        public string CheckRoomAccuracy { get; set; }

        public int RoundAccuracy { get; set; }
        public ObservableCollection<int> RoundAccuracyValues { get; }

        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<LevelViewModel> Levels { get; }

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();
        private void Calculate(object p) {
            // Удаляем все не размещенные помещения
            _revitRepository.RemoveUnplacedRooms();

            // Получаем список дополнительных стадий
            var phases = _revitRepository.GetAdditionalPhases()
                .Select(item => new PhaseViewModel(item, _revitRepository))
                .ToList();
            phases.Add(Phase);

            // Получение всех помещений
            // по заданной стадии
            var levels = Levels.Where(item => item.IsSelected);

            // Проверка всех элементов
            // на выделенных уровнях
            if(CheckElements(levels)) {
                return;
            }

            // Расчет площадей помещений
            CalculateAreas(phases, levels);
        }

        private bool CanCalculate(object p) {
            ErrorText = null;
            return true;
        }

        private bool CheckElements(IEnumerable<LevelViewModel> levels) {
            var errorElements = new Dictionary<ElementId, InfoElementViewModel>();
            foreach(var level in levels) {
                var doors = level.GetDoors(Phase);
                var rooms = level.GetRoomViewModels(Phase);

                // Все двери
                // с не совпадающей секцией
                var notEqualSectionDoors = doors.Where(item => !item.IsSectionNameEqual);
                AddElements("Не совпадают секции у дверей.", notEqualSectionDoors, errorElements);

                // Все помещения которые
                // избыточные или не окруженные
                var redundantRooms = rooms.Where(item => item.IsRedundant == true || item.NotEnclosed == true);
                AddElements("Избыточное или не окруженное помещение.", redundantRooms, errorElements);

                // Все помещения у которых
                // не заполнены обязательные параметры
                var notFilledRequredParamRooms = rooms.Where(item => item.Room == null || item.RoomSection == null || item.RoomGroup == null);
                AddElements("Не заполнены обязательные параметры у помещения.", notFilledRequredParamRooms, errorElements);

                // Все помещения у которых
                // не совпадают значения группы и типа группы
                var notEqualGroupTypeRooms = rooms.Where(room => room.RoomGroup != null)
                    .Where(room => ContainGroups(room))
                    .GroupBy(room => room.RoomGroup)
                    .Where(group => IsGroupTypeEqual(group))
                    .SelectMany(items => items);

                AddElements("Не совпадают значения параметров групп и типа групп параметры у помещения.", notEqualGroupTypeRooms, errorElements);
            }

            return errorElements.Count > 0;
        }

        private void CalculateAreas(List<PhaseViewModel> phases, IEnumerable<LevelViewModel> levels) {
            using(var transaction = _revitRepository.StartTransaction("Расчет площадей")) {
                // Надеюсь будет достаточно быстро отрабатывать :)
                // Подсчет площадей помещений
                foreach(var level in levels) {
                    var rooms = level.GetRoomViewModels(Phase).ToList();
                    foreach(var section in rooms.GroupBy(item => item.RoomSection.Name)) {
                        foreach(var flat in section.GroupBy(item => item.RoomGroup.Name)) {
                            double apartmentArea = 0;
                            double apartmentAreaRatio = 0;
                            double apartmentLivingArea = 0;
                            double apartmentAreaNoBalcony = 0;

                            double area = 0;

                            foreach(var room in flat) {
                                if(room.IsRoomBalcony == true) {
                                    apartmentLivingArea += ConvertValueToSquareMeters(room.RoomArea);
                                }

                                if(room.IsRoomLiving == true) {
                                    apartmentAreaNoBalcony += ConvertValueToSquareMeters(room.RoomArea);
                                }

                                apartmentArea += ConvertValueToSquareMeters(room.RoomArea);
                                apartmentAreaRatio += ConvertValueToSquareMeters(room.AreaWithRatio);

                                if(room.Phase.Name == "Межквартирные перегородки") {
                                    area += ConvertValueToSquareMeters(room.RoomArea);
                                }
                            }

                            foreach(var room in flat) {
                                room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentArea, ConvertValueToInternalUnits(apartmentLivingArea));
                                room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaRatio, ConvertValueToInternalUnits(apartmentAreaRatio));
                                room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentLivingArea, ConvertValueToInternalUnits(apartmentLivingArea));
                                room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaNoBalcony, ConvertValueToInternalUnits(apartmentAreaNoBalcony));

                                if(IsSpotCalcArea) {
                                    room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentFullArea, ConvertValueToInternalUnits(area));
                                }
                            }
                        }
                    }
                }

                var bigChangesRooms = new Dictionary<ElementId, InfoElementViewModel>();
                foreach(var level in levels) {
                    // Обновление параметра
                    // площади с коэффициентом у зон
                    foreach(var area in level.GetAreas()) {
                        UpdateRoomArea(area);
                    }

                    foreach(var room in level.GetRoomViewModels(phases)) {
                        // Заполняем дублирующие
                        // общие параметры
                        room.UpdateSharedParams();
                    }

                    // Обновление значений в помещениях
                    foreach(var room in level.Rooms) {
                        // Обновляем общий параметр этажа
                        room.UpdateLevelSharedParam();

                        // Обновление параметра
                        // площади с коэффициентом
                        var roomAreaWithRatio = ConvertValueToSquareMeters(room.ComputeRoomAreaWithRatio());
                        room.AreaWithRatio = ConvertValueToInternalUnits(roomAreaWithRatio);

                        var areaOldValue = ConvertValueToSquareMeters(room.Area);
                        var areaNewValue = ConvertValueToSquareMeters(room.RoomArea);

                        room.Area = areaNewValue;
                        if(bool.TryParse(CheckRoomAccuracy, out bool result) && result && areaOldValue > 0) {
                            bool isBigChange = Math.Abs(areaOldValue - areaNewValue) / areaOldValue * 100 > GetRoomAccuracy();
                            if(isBigChange) {
                                AddElement("Большие изменения в площади.", room, bigChangesRooms);
                            }
                        }
                    }
                }

                transaction.Commit();
            }
        }

        private void UpdateRoomArea(Element element) {
            double? squareArea = (double?) element.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA);

            squareArea = ConvertValueToSquareMeters(squareArea);
            squareArea = ConvertValueToInternalUnits(squareArea.Value);

            element.SetParamValue(SharedParamsConfig.Instance.RoomAreaWithRatio, squareArea.Value);
        }

        private static bool IsGroupTypeEqual(IEnumerable<RoomViewModel> rooms) {
            return rooms
                .Select(group => group.RoomTypeGroup.Name)
                .Distinct(StringComparer.CurrentCultureIgnoreCase).Count() != 1;
        }

        private static bool ContainGroups(RoomViewModel item) {
            return new[] { "апартаменты", "квартира", "гостиничный номер", "пентхаус" }
                .Any(group => Contains(item.RoomGroup.Name, group, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool Contains(string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        private int GetRoomAccuracy() {
            return int.TryParse(CheckRoomAccuracy, out int result) ? result : 100;
        }

        private double ConvertValueToSquareMeters(double? value) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS), RoundAccuracy) : 0;
        }

        private double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
        }

        private void AddElements(string infoText, IEnumerable<IElementViewModel<Element>> elements, Dictionary<ElementId, InfoElementViewModel> infoElements) {
            foreach(var element in elements) {
                AddElement(infoText, element, infoElements);
            }
        }

        private void AddElement(string infoText, IElementViewModel<Element> element, Dictionary<ElementId, InfoElementViewModel> infoElements) {
            if(!infoElements.TryGetValue(element.ElementId, out var value)) {
                value = new InfoElementViewModel() { Element = element, Errors = new ObservableCollection<string>() };
                infoElements.Add(element.ElementId, value);
            }

            value.Errors.Add(infoText);
        }
    }
}
