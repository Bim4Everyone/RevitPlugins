using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models;
internal class FamInstanceRoomSeparator : RoomSeparator {
    private readonly ApartmentsProject _project;

    public FamInstanceRoomSeparator(ApartmentsProject project, FamilyInstance door) {
        _project = project;

        AddRoom(door.get_FromRoom(_project.Phase));
        AddRoom(door.get_ToRoom(_project.Phase));
    }

    private void AddRoom(Room room) {
        // У дверей может не быть помещения с одной стороны
        if(room != null) {
            Rooms.Add(room);
        }
    }
}
