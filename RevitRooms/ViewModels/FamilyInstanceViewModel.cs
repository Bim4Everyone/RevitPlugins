using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class FamilyInstanceViewModel : ElementViewModel<FamilyInstance> {
        public FamilyInstanceViewModel(FamilyInstance familyInstance, PhaseViewModel phase, RevitRepository revitRepository) :
            base(familyInstance, revitRepository) {

            Phase = phase;
            try {
                ToRoom = new SpatialElementViewModel(Element.get_ToRoom(Phase.Element), revitRepository);
            } catch { }

            try {
                FromRoom = new SpatialElementViewModel(Element.get_FromRoom(Phase.Element), revitRepository);
            } catch { }
        }

        public PhaseViewModel Phase { get; }

        public SpatialElementViewModel ToRoom { get; }
        public SpatialElementViewModel FromRoom { get; }

        public ElementId LevelId => Element.LevelId;
        public override string LevelName => RevitRepository.GetElement(Element.LevelId)?.Name;
        
        public override string PhaseName {
            get { return Phase.Name; }
        }

        public bool IsSectionNameEqual {
            get {
                if(ToRoom == null || FromRoom == null) {
                    return true;
                }

                return ToRoom.RoomSection?.Id == FromRoom.RoomSection?.Id;
            }
        }

        public bool IsGroupNameEqual {
            get {
                if(ToRoom == null || FromRoom == null) {
                    return true;
                }

                if(ToRoom.RoomGroup?.Name.IndexOf("квартира", StringComparison.CurrentCultureIgnoreCase) < 0
                   || FromRoom.RoomGroup?.Name.IndexOf("квартира", StringComparison.CurrentCultureIgnoreCase) < 0) {
                    return true;
                }

                return ToRoom.RoomGroup?.Id == FromRoom.RoomGroup?.Id;
            }
        }
    }
}
