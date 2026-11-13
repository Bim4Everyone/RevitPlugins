using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitRoomTagPlacement.Models;
internal class RoomFromRevit {
    public RoomFromRevit(Room room, ILocalizationService localizationService, 
                         ElementId linkId = null, Transform transform = null) {
        RoomObject = room;
        LinkId = linkId;
        Transform = transform;
        // Стандартное свойство Name нельзя использовать,
        // так как оно возвращает имя вместе с номером помещения
        string withoutNameText = localizationService.GetLocalizedString("MainWindow.RoomWithoutName");
        Name = RoomObject.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, withoutNameText);
    }

    public Room RoomObject { get; }

    public string Name { get; }

    public ElementId LinkId { get; }
    public Transform Transform { get; }

    public XYZ CenterPoint => RoomObject.ClosedShell
        .OfType<Solid>()
        .First()
        .ComputeCentroid();
}
