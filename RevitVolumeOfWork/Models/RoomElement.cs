using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitVolumeOfWork.Models {
    internal class RoomElement {

        Room _room;

        public RoomElement(Room room) {
            _room = room;
        }

        public Level Level { get => _room.Level;  }
    }
}
