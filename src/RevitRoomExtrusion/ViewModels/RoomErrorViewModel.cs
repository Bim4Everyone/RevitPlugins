using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class RoomErrorViewModel {
        private readonly ILocalizationService _localizationService;
        private readonly RoomChecker _roomChecker;

        public RoomErrorViewModel(
            ILocalizationService localizationService, RoomChecker roomChecker, Room room, ICommand showElementCommand) {
            _roomChecker = roomChecker;
            _localizationService = localizationService;
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
            if(room.IsRedundant()) {
                errorDescription = _localizationService.GetLocalizedString("RoomErrorViewModel.RedundantError");
            } else if(room.IsNotEnclosed()) {
                errorDescription = _localizationService.GetLocalizedString("RoomErrorViewModel.NotEnclosed");
            } else if(_roomChecker.CheckIntersectBoundary(room)) {
                errorDescription = _localizationService.GetLocalizedString("RoomErrorViewModel.IntersectBoundary");
            }
            return errorDescription;
        }

        private string GetParamNumber(Room room) {

            string roomGroupShortName = null;
            string numberPrefix = null;

            SharedParam sharedParam = SharedParamsConfig.Instance.RoomGroupShortName;
            if(room.IsExistsParam(sharedParam)) {
                roomGroupShortName = room.GetParamValue<string>(sharedParam);

                if(room.IsExistsParamValue(sharedParam)) {
                    numberPrefix = $"{roomGroupShortName}-";
                }
            }

            Parameter numberParameter = room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if(numberParameter.AsString() != "") {
                return $"{numberPrefix}{numberParameter.AsString()}";
            } else {
                return _localizationService.GetLocalizedString("RoomErrorViewModel.NoNumber");
            }
        }

        private string GetParamName(Room room) {
            Parameter nameParameter = room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if(nameParameter.AsString() != "") {
                return nameParameter.AsString();
            } else {
                return _localizationService.GetLocalizedString("RoomErrorViewModel.NoName");
            }
        }
    }
}
