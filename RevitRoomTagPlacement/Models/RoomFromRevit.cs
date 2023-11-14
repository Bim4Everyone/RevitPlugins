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
        private readonly Room _room;
        private readonly ElementId _linkId;

        public RoomFromRevit(Room room, ElementId linkId = null) {
            _room = room;
            _linkId = linkId;
        }

        public Room RoomObject => _room;

        // Стандартное свойство Name нельзя использовать,
        // так как оно возвращает имя вместе с номером помещения
        public string Name => _room.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, "<Без имени>");
        public ElementId LinkId => _linkId;
    }
}
