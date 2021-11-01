using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class DoorViewModel : ElementViewModel<FamilyInstance> {
        public DoorViewModel(FamilyInstance door, RevitRepository revitRepository) :
            base(door, revitRepository) {

            Phase = new PhaseViewModel(revitRepository.GetPhase(Element), revitRepository);
            ToRoom = new RoomViewModel(Element.get_ToRoom(Phase.Element), revitRepository);
            FromRoom = new RoomViewModel(Element.get_FromRoom(Phase.Element), revitRepository);
        }

        public PhaseViewModel Phase { get; }
        
        public RoomViewModel ToRoom { get; }
        public RoomViewModel FromRoom { get; }

        public bool IsSectionNameEqual {
            get { return ToRoom?.RoomSection?.Equals(FromRoom?.RoomSection) == true; }
        }
    }
}
