using System;
using System.Collections.Generic;

namespace RevitDeclarations.Models;
internal class PublicArea : RoomGroup {
    private readonly PublicAreasSettings _settings;

    public PublicArea(IEnumerable<RoomElement> rooms,
                      DeclarationSettings settings,
                      RoomParamProvider paramProvider)
        : base(rooms, settings, paramProvider) {
        _settings = (PublicAreasSettings) settings;

        RoomPosition = _paramProvider.GetRoomsPosition(this);

        if(RoomPosition.IndexOf("подземный", StringComparison.OrdinalIgnoreCase) >= 0) {
            IsUnderground = true;
        }
    }

    public string DeclarationNumber =>
        _paramProvider.GetTwoParamsWithHyphen(_firstRoom, _settings.AddPrefixToNumber);

    public override string Department => _paramProvider.GetDepartment(_firstRoom, "МОП");
    public override double AreaMain => _isOneRoomGroup ? _firstRoom.Area : base.AreaMain;

    public string GroupName => _firstRoom.Name;

    public string RoomPosition { get; }
    public bool IsUnderground { get; }
}
