using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
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
            Phase = new PhaseViewModel(_revitRepository.GetPhase(_room));
        }

        public string DisplayData {
            get { return _room.Name; }
        }

        public string RoomTypeGroupName {
            get { return (string) _room.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomTypeGroupName); }
        }
        
        public string RoomName {
            get { return (string) _room.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomName); }
        }
        
        public string RoomGroupName {
            get { return (string) _room.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomGroupName); }
        }
        
        public string RoomSectionName {
            get { return (string) _room.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomSectionName); }
        }

        public string LevelName {
            get { return Level.DisplayData; }
        }

        public double? RoomArea {
            get { return (double?) _room.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA); }
        }

        public bool IsPlaced {
            get { return _room.Location != null; }
        }

        public LevelViewModel Level { get; }
        public PhaseViewModel Phase { get; }
    }
}
