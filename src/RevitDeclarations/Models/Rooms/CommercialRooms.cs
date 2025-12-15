using System.Collections.Generic;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal class CommercialRooms : RoomGroup {
    private readonly CommercialSettings _settings;

    public CommercialRooms(IEnumerable<RoomElement> rooms,
                                       DeclarationSettings settings,
                                       RoomParamProvider paramProvider)
        : base(rooms, settings, paramProvider) {
        _settings = (CommercialSettings) settings;
    }

    [JsonProperty("building_number")]
    public string BuildingNumber => _firstRoom.GetTextParamValue(_settings.BuildingNumberParam);
    [JsonProperty("construction_works")]
    public string ConstrWorksNumber => _firstRoom.GetTextParamValue(_settings.ConstrWorksNumberParam);
    [JsonProperty("ceiling_height")]
    public double RoomsHeight => _firstRoom.GetLengthParamValue(_settings.RoomsHeightParam, _accuracyForLength);

    [JsonProperty("type")]
    public override string Department => _paramProvider.GetDepartment(_firstRoom, "Нежилые помещения");
    public string ParkingSpaceClass => _firstRoom.GetTextParamValue(_settings.ParkingSpaceClass);
    public string ParkingInfo => _firstRoom.GetTextParamValue(_settings.ParkingInfo);

    public string DeclarationNumber =>
        _paramProvider.GetTwoParamsWithHyphen(_firstRoom, _settings.AddPrefixToNumber);

    public override double AreaMain => _isOneRoomGroup ? _firstRoom.Area : base.AreaMain;

    public string GroupName => _isOneRoomGroup ? _firstRoom.Name : _firstRoom.GetTextParamValue(_settings.GroupNameParam);

    public bool IsOneRoomGroup => _isOneRoomGroup;
}
