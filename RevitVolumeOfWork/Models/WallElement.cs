using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.DB;

namespace RevitVolumeOfWork.Models {
    internal class WallElement {

        Element _wall;

        public WallElement(Element wall) {
            _wall = wall;

        }
        public Element Wall { get => _wall ;  }
        public List<RoomElement> Rooms { get; set; }

        public string GetRoomsParameters(string fieldName) {
            PropertyInfo typeField = typeof(RoomElement).GetProperty(fieldName);
            IEnumerable<string> values = Rooms
                .Select(x => (string) typeField?.GetValue(x))
                .Distinct();

            return string.Join("; ", values);
        }


    }
}
