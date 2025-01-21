using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class RoomErrorViewModel {

        private readonly RevitRepository _revitRepository;

        public RoomErrorViewModel(RevitRepository revitRepository, Room room, ICommand showElementCommand) {
            _revitRepository = revitRepository;
            ElementId = room.Id;
            RoomName = GetParamName(room);
            RoomNumber = GetParamNumber(room);
            LevelName = room.Level.Name;
            ErrorDescription = GetErrorDescription(room);
            ShowElementCommand = showElementCommand;
        }

        public ICommand ShowElementCommand { get; set; }
        public ElementId ElementId { get; private set; }
        public string RoomName { get; set; }
        public string RoomNumber { get; set; }
        public string LevelName { get; set; }
        public string ErrorDescription { get; set; }

        private string GetErrorDescription(Room room) {
            string errorDescription = null;
            var roomChecker = new RoomChecker(_revitRepository);

            if(room.IsRedundant()) {
                errorDescription = "Помещение избыточно";
            } else if(room.IsNotEnclosed()) {
                errorDescription = "Помещение не окружено";
            } else if(roomChecker.CheckIntersectBoundary(room)) {
                errorDescription = "Помещение не ограничено разделителями";
            }
            return errorDescription;
        }

        private string GetParamNumber(Room room) {
            string numberPrefix = null;
            string roomGroupShortName = room.GetParamValue<string>(SharedParamsConfig.Instance.RoomGroupShortName);
            if(!room.IsExistsParam(SharedParamsConfig.Instance.RoomGroupShortName)) {
                if(roomGroupShortName != null) {
                    numberPrefix = $"{roomGroupShortName}-";
                }
            }
            Parameter numberParameter = room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if(numberParameter.AsString() != "") {
                return $"{numberPrefix}{numberParameter.AsString()}";
            } else {
                return "Нет номера";
            }
        }

        private string GetParamName(Room room) {
            Parameter nameParameter = room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if(nameParameter.AsString() != "") {
                string roomName = nameParameter.AsString();
                return roomName;
            } else {
                return "Нет имени";
            }
        }
    }
}
