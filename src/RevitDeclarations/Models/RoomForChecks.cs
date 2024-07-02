using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models {
    internal class RoomForChecks {
        private readonly Room _revitRoom;

        public RoomForChecks(Room room) {
            _revitRoom = room;            
        }

        public bool HasParameter(Parameter parameter) {
            if(_revitRoom.LookupParameter(parameter.Definition.Name) == null) {
                return false;
            }
            return true;
        }
    }
}
