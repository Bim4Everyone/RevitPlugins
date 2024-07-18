using System.ComponentModel;

namespace RevitFinishingWalls.Models.Enums {
    internal enum RoomGetterMode {
        [Description("По уже выбранным помещениям")]
        AlreadySelectedRooms,
        [Description("По помещениям на активном виде")]
        RoomsOnActiveView,
        [Description("По выбранным вручную помещениям")]
        ManuallySelectedRooms
    }
}
