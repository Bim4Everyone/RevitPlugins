using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models.Rooms;

internal sealed class RoomBoundaryInfo(Room room, List<BoundarySegment> segments) {
    public Room Room { get; } = room;
    public List<BoundarySegment> Segments { get; } = segments;
}
