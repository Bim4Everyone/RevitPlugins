using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;


namespace RevitRoomAnnotations.Models;
public class RevitRoom {
    private readonly ElementId _roomId;
    private readonly ElementId _linkInstanceId;

    public RevitRoom(SpatialElement room, LinkInstanceElement linkInstanceElement) {
        _roomId = room.Id;
        _linkInstanceId = linkInstanceElement.Id;
        AdditionalNumber = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentNumberExtra.Name);
        AdditionalName = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentNameExtra.Name);
        Area = room.GetParamValue<double>(SharedParamsConfig.Instance.RoomArea.Name);
        AreaWithCoefficient = room.GetParamValue<double>(SharedParamsConfig.Instance.RoomAreaWithRatio.Name);
        GroupSortOrder = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentGroupName.Name);
        RoomCategory = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomFireCategory.Name);
        FireZone = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.FireCompartmentShortName.Name);
        Level = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.Level.Name);
        Group = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomGroupShortName.Name);
        Building = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomBuildingShortName.Name);
        Section = room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomSectionShortName.Name);
    }

    public string CombinedId => $"{_roomId}_{_linkInstanceId}";
    public string AdditionalNumber { get; }
    public string AdditionalName { get; }
    public double? Area { get; }
    public double? AreaWithCoefficient { get; }
    public string GroupSortOrder { get; }
    public string RoomCategory { get; }
    public string FireZone { get; }
    public string Level { get; }
    public string Group { get; }
    public string Building { get; }
    public string Section { get; }
}
