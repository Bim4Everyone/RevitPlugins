using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models {
    internal class CommercialRooms : RoomGroup {
        private readonly bool _isOneRoomGroup = false;

        public CommercialRooms(IEnumerable<RoomElement> rooms, DeclarationSettings settings) 
            : base(rooms, settings) {
            if (rooms.Count() == 1) {
                _isOneRoomGroup = true;
            }
        }

        public bool IsOneRoomGroup => _isOneRoomGroup;
    }
}
