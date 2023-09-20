using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitRoomTagPlacement.ViewModels {
    internal class RoomGroupViewModel : BaseViewModel {

        string _name;
        IEnumerable<Room> _rooms;


        public RoomGroupViewModel(string name, IEnumerable<Room> rooms) {
            _name = name;   
            _rooms = rooms;
        }

        public string Name => _name;
        public IEnumerable<Room> Rooms => _rooms;

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
