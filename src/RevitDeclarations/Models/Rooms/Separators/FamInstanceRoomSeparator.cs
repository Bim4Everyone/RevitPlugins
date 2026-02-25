using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models;
internal class FamInstanceRoomSeparator : RoomSeparator {
    public FamInstanceRoomSeparator(ApartmentsProject project, FamilyInstance door) {
        AddRoom(door.get_FromRoom(project.Phase));
        AddRoom(door.get_ToRoom(project.Phase));
    }

    private void AddRoom(Room room) {
        // У дверей может не быть помещения с одной стороны
        if(room != null) {
            Rooms.Add(room);
        }
    }
}
