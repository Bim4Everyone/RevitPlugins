using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;


namespace RevitRoomAnnotations.Models;
public class RevitRoom {
    private readonly SpatialElement _room;
    private readonly string _linkInstanceName;
    private readonly Document _document;

    public RevitRoom(SpatialElement room, LinkInstanceElement linkInstanceElement, Document document) {
        _room = room;
        _linkInstanceName = linkInstanceElement.Name;
        _document = document;
    }

    public string LinkName => GetLinkName();
    public string AdditionalNumber => GetParamValue<string>(_room, SystemParamsConfig.Instance.CreateRevitParam(_document, BuiltInParameter.ROOM_NUMBER));
    public string AdditionalName => GetParamValue<string>(_room, SystemParamsConfig.Instance.CreateRevitParam(_document, BuiltInParameter.ROOM_NAME));
    public double? Area => GetParamValue<double>(_room, SharedParamsConfig.Instance.RoomArea);
    public double? AreaWithCoefficient => GetParamValue<double>(_room, SharedParamsConfig.Instance.RoomAreaWithRatio);
    public string GroupSortOrder => GetParamValue<string>(_room, SharedParamsConfig.Instance.ApartmentGroupName);
    public string RoomCategory => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomFireCategory);
    public string FireZone => GetParamValue<string>(_room, SharedParamsConfig.Instance.FireCompartmentShortName);
    public string Level => GetParamValue<string>(_room, SharedParamsConfig.Instance.Level);
    public string Group => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomGroupShortName);
    public string Building => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomBuildingShortName);
    public string Section => GetParamValue<string>(_room, SharedParamsConfig.Instance.RoomSectionShortName);

    private T GetParamValue<T>(SpatialElement room, RevitParam revitParam) {
        return room.GetParamValueOrDefault<T>(revitParam.Name);
    }

    private string GetLinkName() {
        return $"LinkName: {_linkInstanceName}";
    }
}
