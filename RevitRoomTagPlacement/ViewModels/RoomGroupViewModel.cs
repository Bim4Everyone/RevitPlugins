using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class RoomGroupViewModel : BaseViewModel {

        string _name;
        IEnumerable<RoomFromRevit> _rooms;
        IEnumerable<string> _groupRoomNames;

        public RoomGroupViewModel(string name, IEnumerable<RoomFromRevit> rooms) {
            _name = name;   
            _rooms = rooms;

            _groupRoomNames = GetRoomNames();
        }

        public string Name => _name;
        public IEnumerable<RoomFromRevit> Rooms => _rooms;

        public IEnumerable<string> GroupRoomNames => _groupRoomNames;

        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<string> GetRoomNames() {
            RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
            var roomsBySection = _rooms
                .GroupBy(r => r.RoomObject.GetParamValue<string>(sectionParam));

            IEnumerable<string> uniqueNames = new List<string>(roomsBySection
                .First()
                .Select(x => x.RoomObject.GetParamValue<string>(BuiltInParameter.ROOM_NAME)));

            foreach(var group in roomsBySection) {
                uniqueNames = uniqueNames.Intersect(group.Select(x => x.RoomObject.GetParamValue<string>(BuiltInParameter.ROOM_NAME)));
            }

            return uniqueNames;
        }

        public List<RoomFromRevit> GetMaxAreaRooms() {
            RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;

            return _rooms
                .GroupBy(r => r.RoomObject.GetParamValue<string>(sectionParam))
                .Select(g => g.OrderByDescending(r => r.RoomObject.Area).First())
                .ToList();
        }

        public List<RoomFromRevit> GetRoomsByName(string roomName) {
            RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;

            return _rooms
                .GroupBy(r => r.RoomObject.GetParamValue<string>(sectionParam))
                .Select(g => g.Where(y => y.RoomObject.GetParamValue<string>(BuiltInParameter.ROOM_NAME) == roomName).First())
                .ToList();

        }
    }
}
