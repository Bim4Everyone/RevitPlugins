using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels;
internal class RoomErrorViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly RoomChecker _roomChecker;

    public RoomErrorViewModel(
        ILocalizationService localizationService, RoomChecker roomChecker, Room room, ICommand showElementCommand) {
        _roomChecker = roomChecker;
        _localizationService = localizationService;
        ElementId = room.Id;
        LevelName = room.Level.Name;
        ErrorDescription = GetErrorDescription(room);
        ShowElementCommand = showElementCommand;
        SetParamNumber(room);
        SetParamName(room);
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
        } else if(room.IsSelfCrossBoundaries() || _roomChecker.CheckIntersectBoundary(room)) {
            errorDescription = _localizationService.GetLocalizedString("RoomErrorViewModel.IntersectBoundary");
        }
        return errorDescription;
    }

    private void SetParamNumber(Room room) {
        string roomGroupShortName = null;
        var shortNameParam = SharedParamsConfig.Instance.RoomGroupShortName;
        if(room.IsExistsParam(shortNameParam)) {
            if(room.IsExistsParamValue(shortNameParam)) {
                roomGroupShortName = $"{room.GetParamValueOrDefault<string>(shortNameParam)}-";
            }
        }
        if(room.IsExistsParam(BuiltInParameter.ROOM_NUMBER)) {
            string defaultValue = _localizationService.GetLocalizedString("RoomErrorViewModel.NoNumber");
            RoomNumber = $"{roomGroupShortName}" +
                $"{room.GetParamValueOrDefault<string>(BuiltInParameter.ROOM_NUMBER, defaultValue)}";
        }
    }

    private void SetParamName(Room room) {
        if(room.IsExistsParam(BuiltInParameter.ROOM_NAME)) {
            string defaultValue = _localizationService.GetLocalizedString("RoomErrorViewModel.NoName");
            RoomName = room.GetParamValueOrDefault<string>(BuiltInParameter.ROOM_NAME, defaultValue);
        }

    }
}
