using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models {
    internal class CommercialRooms : RoomGroup {
        public CommercialRooms(IEnumerable<RoomElement> rooms, DeclarationSettings settings) 
            : base(rooms, settings) {            
        }
    }
}
