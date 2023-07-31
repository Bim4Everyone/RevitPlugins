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
            Category wallCategory = Category.GetCategory(_document, BuiltInCategory.OST_Walls);
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();

            return _room.GetBoundarySegments(options)
                .SelectMany(x => x)
                .Select(x => x.ElementId)
                .Where(x => x != ElementId.InvalidElementId)
                .Select(x => _document.GetElement(x))
                .Where(x => x.Category.Id == wallCategory.Id)
                .ToList();
        } 
    }
}
