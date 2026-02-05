using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomTagPlacement.Models;
internal class RoomTagTypeModel {
    public RoomTagTypeModel(RoomTagType roomTagTypeElement) {
        RoomTagTypeElement = roomTagTypeElement;

        string familyName = RoomTagTypeElement.FamilyName;
        string typeName = roomTagTypeElement.Name;
        Name = string.Format($"{familyName} : {typeName}");
    }

    public string Name { get; }
    public RoomTagType RoomTagTypeElement { get; }
    public ElementId TagId => RoomTagTypeElement.Id;
}
