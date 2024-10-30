using Autodesk.Revit.DB;

namespace RevitRoomViewer.Models {
    internal class RoomElement {
        public ElementId Id { get; set; }
        public ElementId LevelId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool NeedMeasuring { get; set; }
    }
}
