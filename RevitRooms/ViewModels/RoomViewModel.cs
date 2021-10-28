using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class RoomViewModel : BaseViewModel {
        private readonly Room _room;
        private readonly RevitRepository _revitRepository;

        public RoomViewModel(Room room, LevelViewModel levelViewModel, RevitRepository revitRepository) {
            _room = room;
            _revitRepository = revitRepository;

            Level = levelViewModel;
        }

        public string DisplayData {
            get { return _room.Name; }
        }

        public LevelViewModel Level { get; }
    }
}
