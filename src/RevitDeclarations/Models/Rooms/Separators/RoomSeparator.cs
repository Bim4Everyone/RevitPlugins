using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitDeclarations.Models {
    internal abstract class RoomSeparator {
        private readonly BuiltInParameter _bltNameParam = BuiltInParameter.ROOM_NAME;

        public List<Room> Rooms { get; set; } = new List<Room>();

        public bool CheckIsValid() {
            return Rooms.Count > 1;
        }

        public Room GetRoom(RoomPriority priority1) {
            return Rooms
                .Where(x => priority1.CheckName(x.GetParamValue<string>(_bltNameParam)))
                .FirstOrDefault();
        }
    }
}
