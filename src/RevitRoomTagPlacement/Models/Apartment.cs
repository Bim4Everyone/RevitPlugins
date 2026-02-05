using System.Collections.Generic;
using System.Linq;

namespace RevitRoomTagPlacement.Models;
internal class Apartment {
    public Apartment(IEnumerable<RoomFromRevit> rooms) {
        Rooms = rooms.ToList();
        RoomNames = Rooms
            .Select(x => x.Name)
            .Distinct()
            .ToList();
        MaxAreaRooms = Rooms
            .GroupBy(x => x.RoomObject.LevelId)
            .Select(g => g.OrderByDescending(r => r.RoomObject.Area).First())
            .ToList();
    }

    public IReadOnlyCollection<RoomFromRevit> Rooms { get; }
    public IReadOnlyCollection<string> RoomNames { get; }

    /// <summary>
    /// Возвращает помещение с максимальной площадью для каждого уровня.
    /// </summary>
    public IReadOnlyCollection<RoomFromRevit> MaxAreaRooms { get; }

    /// <summary>
    /// Возвращает помещения с заданным именем для каждого уровня.
    /// </summary>
    public IReadOnlyCollection<RoomFromRevit> GetRoomsByName(string roomName) {
        return Rooms
            .GroupBy(x => x.RoomObject.LevelId)
            .Select(g => g.Where(y => y.Name.Equals(roomName)).First())
            .ToList();
    }
}
