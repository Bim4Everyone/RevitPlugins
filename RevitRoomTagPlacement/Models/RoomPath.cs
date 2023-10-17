using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models {
    internal class RoomPath {
        private Room room;

        public RoomPath(Room _room) {
            room = _room;


        }


        private Mesh GetRoomMesh() {
            Solid roomSolid = room.ClosedShell
                .OfType<Solid>()
                .First();

            var faceArray = (IEnumerable<Face>) roomSolid.Faces;

            return faceArray
                .OfType<PlanarFace>()
                .Where(y => y.FaceNormal.Z != 0)
                .First()
                .Triangulate();
        }

    }
}
