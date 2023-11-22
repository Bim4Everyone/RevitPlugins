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

        private readonly IReadOnlyCollection<RoomFromRevit> _rooms;
        private readonly IReadOnlyCollection<string> _roomNames;
        private readonly RoomFromRevit _maxAreaRoom;

        public Apartment(IEnumerable<RoomFromRevit> rooms) {
            _rooms = rooms.ToList();
            _roomNames = _rooms
                .Select(x => x.Name)
                .Distinct()
                .ToList();
            _maxAreaRoom = _rooms
                .OrderByDescending(r => r.RoomObject.Area)
                .First();
        }

        public IReadOnlyCollection<RoomFromRevit> Rooms => _rooms;
        public IReadOnlyCollection<string> RoomNames => _roomNames;
        public RoomFromRevit MaxAreaRoom => _maxAreaRoom;

        public RoomFromRevit GetRoomByName(string roomName) {
            return _rooms
                .Where(y => y.Name.Equals(roomName))
                .First();
        }
    }
}
