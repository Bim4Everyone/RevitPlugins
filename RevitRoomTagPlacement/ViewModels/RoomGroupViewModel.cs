using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitRoomTagPlacement.ViewModels {
    internal class RoomGroupViewModel : BaseViewModel {

        string _name;
        IEnumerable<Room> _rooms;
        IEnumerable<string> _groupRoomNames;

        public RoomGroupViewModel(string name, IEnumerable<Room> rooms) {
            _name = name;   
            _rooms = rooms;

            _groupRoomNames = rooms
                .Select(x => x.GetParamValue<string>(BuiltInParameter.ROOM_NAME))
                .Distinct();
        }

        public string Name => _name;
        public IEnumerable<Room> Rooms => _rooms;

        public IEnumerable<string> GroupRoomNames => _groupRoomNames;

        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                OnPropertyChanged();
            }
        }
    }
}
