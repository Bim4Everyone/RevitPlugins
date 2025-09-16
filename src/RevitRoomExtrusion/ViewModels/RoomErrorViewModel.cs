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
    private readonly Room _room;

    public RoomErrorViewModel(
        ILocalizationService localizationService, RoomChecker roomChecker, Room room, ICommand showElementCommand) {
        _localizationService = localizationService;
        _roomChecker = roomChecker;
        _room = room;

        ShowElementCommand = showElementCommand;
    }

    public ICommand ShowElementCommand { get; set; }
    public ElementId ElementId => _room.Id;
    public string LevelName => _room.Level.Name;
    public string RoomName => GetRoomName(_room);
    public string RoomNumber => GetRoomNumber(_room);
    public string ErrorDescription => GetRoomErrorDescription(_room);

    // Метод получения имени помещения
    private string GetRoomName(Room room) {
        string defaultValue = _localizationService.GetLocalizedString("RoomErrorViewModel.NoName");
        return room.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, defaultValue);
    }

    // Метод получения номера помещения
    private string GetRoomNumber(Room room) {
        string number = room.GetParamValueOrDefault<string>(
            BuiltInParameter.ROOM_NUMBER,
            _localizationService.GetLocalizedString("RoomErrorViewModel.NoNumber"));
        string prefix = GetRoomNumberPrefix(room);
        return string.IsNullOrEmpty(prefix) ? number : $"{prefix}-" + $"{number}";
    }

    // Метод получения префикса номера помещения
    private string GetRoomNumberPrefix(Room room) {
        return room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomGroupShortName);
    }

    // Метод типа ошибки помещения
    private string GetRoomErrorDescription(Room room) {
        return room.IsRedundant()
            ? _localizationService.GetLocalizedString("RoomErrorViewModel.RedundantError")
            : room.IsNotEnclosed()
            ? _localizationService.GetLocalizedString("RoomErrorViewModel.NotEnclosed")
            : (room.IsSelfCrossBoundaries() || _roomChecker.CheckEqualBoundary(room))
            ? _localizationService.GetLocalizedString("RoomErrorViewModel.IntersectBoundary")
            : null;
    }
}
