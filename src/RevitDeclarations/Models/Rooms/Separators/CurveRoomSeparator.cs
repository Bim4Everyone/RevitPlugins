using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

namespace RevitDeclarations.Models {
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
}
