using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitVolumeOfWork.Models; 
internal class WallElement {
    public WallElement(Element wall) {
        Wall = wall;

    }
    public Element Wall { get; }
    public List<RoomElement> Rooms { get; set; }

    public string GetRoomsParameters(string fieldName) {
        var typeField = typeof(RoomElement).GetProperty(fieldName);
        var values = Rooms
            .Select(x => (string) typeField?.GetValue(x))
            .Distinct();

        return string.Join("; ", values);
    }

    public override bool Equals(object obj) {
        return Equals(obj as WallElement);
    }

    public bool Equals(WallElement wallElement) {
        return Wall.Id == wallElement.Wall.Id;
    }

    public override int GetHashCode() {
        return Wall.Id.GetHashCode();
    }
}
