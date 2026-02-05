using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal class CurveRoomSeparator : RoomSeparator {
    private readonly CurveElement _curve;

    public CurveRoomSeparator(ApartmentsProject project, CurveElement curve) {
        _curve = curve;

        foreach(var room in project.Rooms) {
            AddRoom(room);
        }
    }

    private void AddRoom(RoomElement room) {
        if(room.GetBoundaries().Contains(_curve.Id)) {
            Rooms.Add(room.RevitRoom);
        }
    }
}
