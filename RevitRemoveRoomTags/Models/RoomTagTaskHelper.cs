using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

namespace RevitRemoveRoomTags.Models {
    internal class RoomTagTaskHelper {

        public RoomTagTaskHelper() {

        }
        public List<RoomTag> RoomTags { get; set; } = new List<RoomTag>();

        public string XOffset { get; set; } = "0";
        public string YOffset { get; set; } = "0";

        public bool RemoveTags { get; set; } = false;
    }
}
