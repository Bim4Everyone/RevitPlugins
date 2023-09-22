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

        public RoomTagTypeModel(RoomTagType roomTagTypeElement) {
            RoomTagTypeElement = roomTagTypeElement;
            
            string familyName = RoomTagTypeElement.FamilyName;
            string typeName = roomTagTypeElement.Name;
            Name = string.Format("{0} : {1}", familyName, typeName);
        }

        public string Name { get; }
        public RoomTagType RoomTagTypeElement { get; }

        public ElementId TagId => RoomTagTypeElement.Id;
    }
}
