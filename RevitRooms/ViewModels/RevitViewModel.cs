using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
            Phases = new ObservableCollection<PhaseViewModel>(Levels.SelectMany(item => item.Rooms).Select(item => item.Phase));

            RoundAccuracy = 1;
            RoundAccuracyValues = new ObservableCollection<int>(Enumerable.Range(1, 3));
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
                .Select(item => new RoomViewModel(item, _revitRepository));
        }
    }
}
