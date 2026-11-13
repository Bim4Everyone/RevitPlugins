using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models.Rooms;

internal sealed class RoomBoundaryCache {
    private readonly Dictionary<ElementId, Dictionary<ElementId, RoomBoundaryInfo>> _cache = [];

    public void Add(ElementId curveId, Room room, BoundarySegment segment) {
        if (!_cache.TryGetValue(curveId, out var rooms)) {
            rooms = [];
            _cache.Add(curveId, rooms);
        }

        if (!rooms.TryGetValue(room.Id, out var roomInfo)) {
            roomInfo = new RoomBoundaryInfo(room, []);
            rooms.Add(room.Id, roomInfo);
        }

        roomInfo.Segments.Add(segment);
    }

    public bool TryGetRooms(ElementId curveId, out Dictionary<ElementId, RoomBoundaryInfo> rooms) {
        return _cache.TryGetValue(curveId, out rooms);
    }
}
