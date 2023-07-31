using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

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

            return String.Join(",", values);
        }


    }
}
