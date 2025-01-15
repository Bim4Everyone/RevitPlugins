using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;


namespace RevitRoomExtrusion.Models {
    internal class ErrorRoomElement { 
        
        private readonly RevitRepository _revitRepository;

        public ErrorRoomElement(RevitRepository revitRepository, Room room, ICommand showElementCommand) {
            _revitRepository = revitRepository;
            ElementId = room.Id;
            Name = room.Name;
            NumberRoom = GetParamNumber(room);
            LevelName = room.Level.Name;
            ErrorDescription = GetErrorDescription(room);
            ShowElementCommand = showElementCommand;
        }

        public ICommand ShowElementCommand { get; set; }
        public ElementId ElementId { get; set; }
        public string Name { get; set; }
        public string NumberRoom { get; set; }
        public string LevelName { get; set; }
        public string ErrorDescription { get; set; }

        private string GetErrorDescription(Room room) {
            string errorDescription = null;
            RoomChecker roomChecker = new RoomChecker(_revitRepository);

            if(room.IsRedundant()) {
                errorDescription = "Помещение избыточно";
            } else if(room.IsNotEnclosed()) {
                errorDescription = "Помещение не окружено";
            } else if(roomChecker.IsIntersectBoundary(room)) {
                errorDescription = "Помещение не ограничено разделителями";
            }
            return errorDescription;
        }

        private string GetParamNumber(Room room) {
            string roomGroupShortName = room.GetParamValue<string>(SharedParamsConfig.Instance.RoomGroupShortName);
            string numberPrefix = null;
            if(roomGroupShortName != null) {
                numberPrefix = $"{roomGroupShortName}-";
            }
            string numberRoom = $"{numberPrefix}{room.Number}";
            return numberRoom;
        }        
    }
}
