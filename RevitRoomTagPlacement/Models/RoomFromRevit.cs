using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

namespace RevitRoomTagPlacement.Models {
    internal class RoomFromRevit {
        private Room _room;
        private ElementId _linkId;
        public RoomFromRevit(Room room, ElementId linkId = null) {
            _room = room;
            _linkId = linkId;
        }

        public bool IsFromLink { get; set; }
        public Room RoomObject => _room;
        public string Name => _room.GetParamValue<string>(BuiltInParameter.ROOM_NAME);
        public ElementId LinkId => _linkId;
    }
}
