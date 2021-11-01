using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class DoorViewModel : BaseViewModel {
        private readonly FamilyInstance _door;

        public DoorViewModel(FamilyInstance door, Phase phase) {
            _door = door;

            ToRoom = new RoomViewModel(_door.get_ToRoom(phase), phase);
            FromRoom = new RoomViewModel(_door.get_FromRoom(phase), phase);
        }

        public RoomViewModel ToRoom { get; }
        public RoomViewModel FromRoom { get; }

        public bool IsSectionNameEqual {
            get { return ToRoom?.RoomSectionName?.Equals(FromRoom?.RoomSectionName) == true; }
        }
    }
}
