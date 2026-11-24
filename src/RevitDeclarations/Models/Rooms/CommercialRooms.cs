using System.Collections.Generic;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitDeclarations.Comparators;

namespace RevitDeclarations.Models;
internal class CommercialRooms : RoomGroup {
    private readonly CommercialSettings _settings;

    public CommercialRooms(IEnumerable<RoomElement> rooms,
                                       DeclarationSettings settings,
                                       RoomParamProvider paramProvider, 
                                       RevitLogicalStringComparer logicalStrComparer)
        : base(rooms, settings, paramProvider, logicalStrComparer) {
        _settings = (CommercialSettings) settings;
    }

    [JsonProperty("building_number")]
    public string BuildingNumber => _firstRoom.GetTextParamValue(_settings.BuildingNumberParam);
    [JsonProperty("construction_works")]
    public string ConstrWorksNumber => _firstRoom.GetTextParamValue(_settings.ConstrWorksNumberParam);
    [JsonProperty("ceiling_height")]
    public double RoomsHeight => _firstRoom.GetLengthParamValue(_settings.RoomsHeightParam, _accuracyForLength);

    [JsonProperty("type")]
    public string ParkingSpaceClass => _firstRoom.GetTextParamValue(_settings.ParkingSpaceClass);
    public string ParkingInfo => GetNameWithMainParkingNumber(
        _firstRoom.GetTextParamValue(_settings.ParkingInfo),
        _settings.ParkingInfoMainNumberParam, _settings.ParkingInfoKeyword);
    public string PositionType => _firstRoom.GetTextParamValue(_settings.PositionType);

    public string DeclarationNumber =>
        _paramProvider.GetTwoParamsWithHyphen(_firstRoom, _settings.AddPrefixToNumber, _settings.AddHyphenToPrefix);

    public override double AreaMain => _isOneRoomGroup ? _firstRoom.Area : base.AreaMain;

    public string GroupName => GetNameWithMainParkingNumber(
        _isOneRoomGroup ? _firstRoom.Name : _firstRoom.GetTextParamValue(_settings.GroupNameParam),
        _settings.GroupNameMainNumberParam, _settings.GroupNameKeyword);

    public bool IsOneRoomGroup => _isOneRoomGroup;

    private string GetNameWithMainParkingNumber(string name, Parameter mainNumberParam, string keyword) {
        if(mainNumberParam == null || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(keyword)) {
            return name;
        }

        string pattern = $@"(?<!\w){Regex.Escape(keyword.Trim())}(?!\w)";
        if(!Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) {
            return name;
        }

        string mainNumber = _firstRoom.GetTextParamValue(mainNumberParam);
        return string.IsNullOrWhiteSpace(mainNumber) ? name : $"{name} от ММ {mainNumber.Trim()}";
    }
}
