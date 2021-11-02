using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class LevelViewModel : ElementViewModel<Level> {
        private bool _isSelected;

        public LevelViewModel(Level level, RevitRepository revitRepository, IEnumerable<Room> rooms) : base(level, revitRepository) {
            Rooms = new ObservableCollection<RoomViewModel>(rooms.Select(item => new RoomViewModel(item, revitRepository)).Where(item => item.IsPlaced));
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public bool IsLivingLevel {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS) > 2; }
        }

        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS) + " " + UnitUtils.GetTypeCatalogString(DisplayUnitType.DUT_METERS); }
        }

        public int RoomsCount {
            get { return Rooms.Count; }
        }

        public ObservableCollection<RoomViewModel> Rooms { get; }

        public IEnumerable<Area> GetAreas() {
            return RevitRepository.GetAllAreas()
            .Where(item => item.LevelId == Element.Id);
        }

        public IEnumerable<DoorViewModel> GetDoors(PhaseViewModel phase) {
            return RevitRepository.GetDoors()
                .Select(item => new DoorViewModel(item, RevitRepository))
                .Where(item => item.Phase.Equals(phase))
                .Where(item => item.LevelId == Element.Id);
        }

        public IEnumerable<RoomViewModel> GetRoomViewModels(PhaseViewModel phase) {
            var phases = RevitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, RevitRepository));
            return GetAdditionalRooms(new[] { phase }.Union(phases));
        }

        private IEnumerable<RoomViewModel> GetAdditionalRooms(IEnumerable<PhaseViewModel> phases) {
            return RevitRepository.GetAllRooms()
                .Select(item => new RoomViewModel(item, RevitRepository))
                .Where(item => item.LevelId == Element.Id)
                .Where(item => phases.Contains(item.Phase));
        }
    }
}
