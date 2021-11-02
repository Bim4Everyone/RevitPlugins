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

            // Получение всех помещений
            // по заданной стадии
            var levels = Levels.Where(item => item.IsSelected);
            var errorElements = new List<IElementViewModel<Element>>();
            foreach(var level in levels) {
                var doors = level.GetDoors(Phase);
                var rooms = level.GetRoomViewModels(Phase);

                // Все двери
                // с не совпадающей секцией
                var notEqualSectionDoors = doors.Where(item => !item.IsSectionNameEqual);
                errorElements.AddRange(notEqualSectionDoors);

                // Все помещений которые
                // избыточные или не окруженные
                var redundantRooms = rooms.Where(item => item.IsRedundant == true || item.NotEnclosed == true);
                errorElements.AddRange(redundantRooms);

                // Все помещения у которых
                // не заполнены обязательные параметры
                var notFilledRequredParamRooms = rooms.Where(item => item.Room == null || item.RoomSection == null || item.RoomGroup == null);
                errorElements.AddRange(notFilledRequredParamRooms);

                // Все помещения у которых
                // не совпадают значения группы и типа группы
                var notEqualGroupTypeRooms = rooms.Where(room => room.RoomGroup != null)
                    .Where(room => ContainGroups(room))
                    .GroupBy(room => room.RoomGroup)
                    .Where(group => IsGroupTypeEqual(group))
                    .SelectMany(items => items)
                    .ToList();

                errorElements.AddRange(errorElements);
            }

            errorElements = errorElements.Distinct().ToList();
            if(errorElements.Count > 0) {
                return;
            }

            foreach(var level in levels) {
                // Обновление параметра
                // площади с коэффициентом у зон
                foreach(var area in level.GetAreas()) {  
                    UpdateRoomArea(area);
                }

                // Обновление значений в помещениях
                foreach(var room in level.GetRoomViewModels(Phase)) {
                    // Заполняем дублирующие
                    // общие параметры
                    room.UpdateSharedParams();

                    // Обновление параметра
                    // площади с коэффициентом
                    UpdateRoomArea(room.Element);
                }
            }

            levels.
        }

        private bool CanCalculate(object p) {
            ErrorText = null;
            return true;
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

        private int? GetRoomAccuracy() {
            return int.TryParse(CheckRoomAccuracy, out int result) ? result : (int?) null;
        }

        private double ConvertValueToSquareMeters(double? value) {
            return value.HasValue ? Math.Round(UnitUtils.ConvertFromInternalUnits(value.Value, DisplayUnitType.DUT_SQUARE_METERS), GetRoomAccuracy().Value) : 0;
        }

        private double ConvertValueToInternalUnits(double value) {
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
        }
    }
}
