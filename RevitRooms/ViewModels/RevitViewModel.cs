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
            var rooms = Levels
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Rooms)
                .Where(item => item.Phase.Equals(Phase))
                .ToList();

            // Все двери у которых разное значение
            // параметра Секции
            var doors = GetErrorDoorsViewModels(rooms);

            // Получение всех помещений
            // по вспомогательным стадиям
            var additionalRooms = GetAdditionalRoomsViewModels();

            // Следующие проверки
            // должны обрабатывать все помещения
            rooms = rooms.Union(additionalRooms).ToList();

            // Все помещений которые
            // избыточные или не окруженные
            var redundantRooms = rooms.Where(item => item.IsRedundant == true
                || item.NotEnclosed == true)
                .ToList();

            // Все помещения у которых
            // не совпадают значения группы и типа группы
            var groupErrorRooms = GetErrorRoomsGroup(rooms);

            // Все помещения у которых
            // не заполнены обязательные параметры
            var errorRooms = rooms.Where(item => item.Room == null
                || item.RoomSection == null
                || item.RoomGroup == null)
                .ToList();

            UpdateAreaSquare();

            foreach(var room in rooms) {
                // Заполняем дублирующие
                // общие параметры
                room.UpdateSharedParams();

                // Обновление параметра
                // площади с коэффициентом
                UpdateRoomArea(room.Element);
            }
        }

        private bool CanCalculate(object p) {
            ErrorText = null;
            return true;
        }

        private void UpdateAreaSquare() {
            var areas = _revitRepository.GetAreas();

            // TODO: Добавить учет этажей
            foreach(var area in areas) {
                UpdateRoomArea(area);
            }
        }

        private void UpdateRoomArea(Element element) {
            double? squareArea = (double?) element.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA);

            squareArea = ConvertValueToSquareMeters(squareArea);
            squareArea = ConvertValueToInternalUnits(squareArea.Value);

            element.SetParamValue(SharedParamsConfig.Instance.RoomAreaWithRatio, squareArea.Value);
        }

        private IEnumerable<RoomViewModel> GetAdditionalRoomsViewModels() {
            var phases = _revitRepository.GetAdditionalPhases();
            return _revitRepository.GetRooms(phases)
                .Select(item => new RoomViewModel(item, _revitRepository));
        }

        private List<DoorViewModel> GetErrorDoorsViewModels(IEnumerable<RoomViewModel> rooms) {
            return _revitRepository.GetDoors()
                .Select(item => new DoorViewModel(item, _revitRepository))
                .Where(item => item.Phase.Equals(Phase))
                .Where(item => !item.IsSectionNameEqual)
                .Where(item => rooms.Contains(item.FromRoom) || rooms.Contains(item.ToRoom))
                .ToList();
        }

        private List<RoomViewModel> GetErrorRoomsGroup(IEnumerable<RoomViewModel> rooms) {
            return rooms.Where(room => room.RoomGroup != null)
                .Where(room => ContainGroups(room))
                .GroupBy(room => room.RoomGroup)
                .Where(group => IsGroupTypeEqual(group))
                .SelectMany(items => items)
                .ToList();
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
