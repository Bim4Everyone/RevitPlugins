using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models {
    internal class RoomTagTypeModel {
        private readonly string _name;
        private readonly RoomTagType _roomTagTypeElement;

        public RoomTagTypeModel(RoomTagType roomTagTypeElement) {
            _roomTagTypeElement = roomTagTypeElement;
            
            string familyName = RoomTagTypeElement.FamilyName;
            string typeName = roomTagTypeElement.Name;
            _name = string.Format($"{familyName} : {typeName}");
        }

        public string Name => _name;
        public RoomTagType RoomTagTypeElement => _roomTagTypeElement;
        public ElementId TagId => _roomTagTypeElement.Id;
    }
}
