using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;


namespace RevitRoomAnnotations.Models;
public class RevitRoom {
    private const string _roomNumber = "Номер";
    private const string _roomName = "Имя";
    private readonly SpatialElement _room;
    private readonly ElementId _linkInstanceId;

    public RevitRoom(SpatialElement room, LinkInstanceElement linkInstanceElement) {
        _room = room;
        _linkInstanceId = linkInstanceElement.Id;
    }

    public string CombinedId => GetCombinedId();
    public string AdditionalNumber => GetParamValue<string>(_room, _roomNumber);
    public string AdditionalName => GetParamValue<string>(_room, _roomName);
    public double? Area => GetParamValue<double>(_room, SharedParamsConfig.Instance.RoomAreaWithRatio.Name);
    public double? AreaWithCoefficient => GetParamValue<double>(_room, SharedParamsConfig.Instance.ApartmentGroupName.Name);
    public string GroupSortOrder => GetParamValue<string>(_room, SharedParamsConfig.Instance.ApartmentGroupName.Name);
    public string RoomCategory => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomFireCategory.Name);
    public string FireZone => GetParamValue<string>(_room, SharedParamsConfig.Instance.FireCompartmentShortName.Name);
    public string Level => GetParamValue<string>(_room, SharedParamsConfig.Instance.Level.Name);
    public string Group => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomGroupShortName.Name);
    public string Building => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomBuildingShortName.Name);
    public string Section => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomSectionShortName.Name);

    private T GetParamValue<T>(SpatialElement room, string paramName) {
        return room.GetParamValueOrDefault<T>(paramName);
    }

    private string GetCombinedId() {
        return $"LinkId: {_linkInstanceId}";
    }
}
