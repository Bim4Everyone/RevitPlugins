using Autodesk.Revit.DB.Architecture;

namespace RevitRoomViewer.Models {
    internal class RoomElement {

        private readonly Room _room;

        public RoomElement(Room room) {
            _room = room;
        }
        public string Id => _room.Id.ToString();
        public string LevelId => _room.LevelId.ToString();
        public string Name => _room.Name;
        public string Description { get; set; }
        public bool NeedMeasuring { get; set; }
    }
}
