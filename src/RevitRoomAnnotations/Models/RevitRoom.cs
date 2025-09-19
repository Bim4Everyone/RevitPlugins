using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitRoomAnnotations.Models;

public class RevitRoom {
    private readonly SpatialElement _room;
    private readonly RevitLinkInstance _sourceLink;

    public RevitRoom(SpatialElement room, RevitLinkInstance sourceLink) {
        _room = room ?? throw new ArgumentNullException(nameof(room));
        _sourceLink = sourceLink ?? throw new ArgumentNullException(nameof(sourceLink));

        RoomId = _room.Id;
        SourceLinkInstanceId = _sourceLink.Id;
        SourceLinkName = _sourceLink.Name;

        FileName = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.FopId.Name);
        AdditionalNumber = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentNumberExtra.Name);
        AdditionalName = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentNameExtra.Name);
        Area = _room.GetParamValue<double>(SharedParamsConfig.Instance.RoomArea.Name);
        AreaWithCoefficient = _room.GetParamValue<double>(SharedParamsConfig.Instance.RoomAreaWithRatio.Name);
        GroupSortOrder = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.ApartmentGroupName.Name);
        RoomCategory = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomFireCategory.Name);
        FireZone = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.FireCompartmentShortName.Name);
        Level = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.Level.Name);
        Group = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomGroupShortName.Name);
        Building = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomBuildingShortName.Name);
        Section = _room.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomSectionShortName.Name);
    }

    public ElementId RoomId { get; }
    public ElementId SourceLinkInstanceId { get; }
    public string SourceLinkName { get; }

    public string FileName { get; }
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
