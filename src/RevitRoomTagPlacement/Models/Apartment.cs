using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRoomTagPlacement.Models {
    internal class Apartment {
        /// <summary>
        /// При работе со связями класс хранит помещения одной группы со всех уровней, 
        /// так как для связей невозможно отфильтровать помещения для активного вида.
        /// </summary>
        private readonly IReadOnlyCollection<RoomFromRevit> _rooms;
        private readonly IReadOnlyCollection<string> _roomNames;
        private readonly IReadOnlyCollection<RoomFromRevit >_maxAreaRooms;

        public Apartment(IEnumerable<RoomFromRevit> rooms) {
            _rooms = rooms.ToList();
            _roomNames = _rooms
                .Select(x => x.Name)
                .Distinct()
                .ToList();
            _maxAreaRooms = _rooms
                .GroupBy(x => x.RoomObject.LevelId)
                .Select(g => g.OrderByDescending(r => r.RoomObject.Area).First())
                .ToList();
        }

        public IReadOnlyCollection<RoomFromRevit> Rooms => _rooms;
        public IReadOnlyCollection<string> RoomNames => _roomNames;

        /// <summary>
        /// Возвращает помещение с максимальной площадью для каждого уровня.
        /// </summary>
        public IReadOnlyCollection<RoomFromRevit> MaxAreaRooms => _maxAreaRooms;

        /// <summary>
        /// Возвращает помещения с заданным именем для каждого уровня.
        /// </summary>
        public IReadOnlyCollection<RoomFromRevit> GetRoomsByName(string roomName) {
            return _rooms
                .GroupBy(x => x.RoomObject.LevelId)
                .Select(g => g.Where(y => y.Name.Equals(roomName)).First())
                .ToList();
        }
    }
}
