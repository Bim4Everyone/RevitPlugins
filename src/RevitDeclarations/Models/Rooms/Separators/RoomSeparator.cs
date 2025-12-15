using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitDeclarations.Models;
internal abstract class RoomSeparator {
    public List<Room> Rooms { get; set; } = [];

    public bool CheckIsValid() {
        return Rooms.Count > 1;
    }

    public Room GetRoom(RoomPriority priority) {
        return Rooms
            .Where(x => priority.CheckName(x.GetParamValue<string>(BuiltInParameter.ROOM_NAME)))
            .FirstOrDefault();
    }
}
