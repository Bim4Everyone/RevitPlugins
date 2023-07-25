using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitVolumeOfWork.Models {
    internal class RoomElement {

        Room _room;
        Document _document;

        public RoomElement(Room room, Document document) {
            _room = room;
            _document = document;
        }

        public Level Level { get => _room.Level;  }

        public string Name { get => _room.Name; }
        public string Number { get => _room.Number; }
        public string ID { get => _room.Id.ToString(); }
        public string ApartNumber { get => _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentNumber); }

        public List<Element> GetBoundaryWalls() {
            List<Element> boundaryWalls = new List<Element>();

            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            var boundaries = _room.GetBoundarySegments(options);
            foreach(var contour in boundaries) {
                foreach(var element in contour) {
                    ElementId elementId = element.ElementId;
                    if(elementId != null) {
                        boundaryWalls.Add(_document.GetElement(elementId));
                    }
                }
            }

            return boundaryWalls;
        } 

    }
}
