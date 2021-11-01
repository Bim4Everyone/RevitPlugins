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

        public string DisplayData { get; set; }
        public PhaseViewModel Phase { get; set; }

        public bool IsSpotCalcArea { get; set; }
        public bool IsCheckRoomsChanges { get; set; }
        public string CheckRoomAccuracy { get; set; }

        public int RoundAccuracy { get; set; }
        public ObservableCollection<int> RoundAccuracyValues { get; }

        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<LevelViewModel> Levels { get; }

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();
        protected virtual IEnumerable<RoomViewModel> GetAdditionalRoomsViewModels() {
            var phases = _revitRepository.GetAdditionalPhases();
            return _revitRepository.GetRooms(phases)
                .Select(item => new RoomViewModel(item, _revitRepository.GetPhase(item)));
        }

        private List<DoorViewModel> GetErrorDoorsViewModels(List<RoomViewModel> rooms) {
            return _revitRepository.GetDoors()
                .Select(item => new DoorViewModel(item, Phase.Element))
                .Where(item => !item.IsSectionNameEqual)
                .Where(item => rooms.Contains(item.FromRoom) || rooms.Contains(item.ToRoom))
                .ToList();
        }

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
            var redundantRooms = rooms.Union(additionalRooms)
                .Where(item => item.IsRedundant == true || item.NotEnclosed == true)
                .ToList();

            // Все помещения у которых
            // не заполнены обязательные параметры
            var errorRooms = rooms.Union(additionalRooms)
                .Where(item => item.RoomName == null || item.RoomSectionName == null || item.RoomGroupName == null)
                .ToList();
        }

        private bool CanCalculate(object p) {
            ErrorText = null;
            return true;
        }
    }
}
