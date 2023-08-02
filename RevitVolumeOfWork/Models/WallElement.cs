using System;
using System.Collections.Generic;
using System.Linq;

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
            List<string> values = Rooms.Select(x => (string) x.GetType().GetProperty(fieldName).GetValue(x))
                                       .Distinct()
                                       .ToList();

            return string.Join(", ", values);
        }


    }
}
