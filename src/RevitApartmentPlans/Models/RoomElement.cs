using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitApartmentPlans.Models {
    internal class RoomElement {
        public RoomElement(Room room, Transform transform) {
            Room = room ?? throw new System.ArgumentNullException(nameof(room));
            Transform = transform ?? throw new System.ArgumentNullException(nameof(transform));
        }

        public RoomElement(Room room) : this(room, Transform.Identity) { }


        public Room Room { get; }
        public Transform Transform { get; }

        public Reference GetReference() {
            return new Reference(Room);
        }
    }
}
