using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitRoomTagPlacement.Models {
    internal class Apartment {

        private readonly IEnumerable<RoomFromRevit> _rooms;
        private readonly IEnumerable<string> _roomNames;

        public Apartment(IEnumerable<RoomFromRevit> rooms) {
            _rooms = rooms;
            _roomNames = rooms.Select(x => x.Name).Distinct();
        }

        public IEnumerable<RoomFromRevit> Rooms => _rooms;
        public IEnumerable<string> RoomNames => _roomNames;

        public RoomFromRevit GetMaxAreaRoom() {
            return _rooms.OrderByDescending(r => r.RoomObject.Area).First();
        }

        public RoomFromRevit GetRoomByName(string roomName) {       
            return _rooms.Where(y => y.Name.Equals(roomName)).First();
        }
    }
}
