using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
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

        public Level Level => _room.Level; 
        public string Name => _room.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, "<Пусто>"); 
        public string Number => _room.GetParamValueOrDefault(BuiltInParameter.ROOM_NUMBER, "<Пусто>");       
        public string Group { 
            get { 
                var keyParamValueId = _room.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomGroupName, ElementId.InvalidElementId);
                if(keyParamValueId == ElementId.InvalidElementId) return "<Пусто>";
                else return _document.GetElement(keyParamValueId).Name;
            } 
        }
        public string ID => _room.Id.ToString();

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
