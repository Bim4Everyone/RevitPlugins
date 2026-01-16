using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal abstract class RoomGroup {
    protected const double _maxAreaDeviation = 0.2;

    protected readonly StringComparer _strComparer = StringComparer.OrdinalIgnoreCase;

    protected readonly RoomParamProvider _paramProvider;
    protected readonly int _accuracyForArea;
    protected readonly int _accuracyForLength;

    protected readonly IEnumerable<RoomElement> _rooms;

    protected readonly RoomElement _firstRoom;
    protected readonly bool _isOneRoomGroup = false;
    private readonly DeclarationSettings _settings;

    public RoomGroup(IEnumerable<RoomElement> rooms, DeclarationSettings settings, RoomParamProvider paramProvider) {
        _settings = settings;
        _paramProvider = paramProvider;
        _accuracyForArea = settings.AccuracyForArea;
        _accuracyForLength = settings.AccuracyForLength;

        _rooms = rooms.ToList();
        _firstRoom = rooms.FirstOrDefault();
        if(rooms.Count() == 1) {
            _isOneRoomGroup = true;
        }
    }

    [JsonProperty("rooms")]
    public IEnumerable<RoomElement> Rooms => _rooms;
    [JsonProperty("type")]
    public virtual string Department => _firstRoom.GetTextParamValue(_settings.DepartmentParam);

    [JsonProperty("floor_number")]
    public string Level => _paramProvider.GetAllLevels(_rooms);
    [JsonProperty("number")]
    public virtual string Number => _firstRoom.GetTextParamValue(_settings.ApartmentNumberParam);
    [JsonProperty("section")]
    public string Section => _firstRoom.GetTextParamValue(_settings.SectionParam);
    [JsonProperty("building")]
    public string Building => _firstRoom.GetTextParamValue(_settings.BuildingParam);

    [JsonProperty("area")]
    public virtual double AreaMain => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracyForArea);

    // Проверка актуальности площадей помещений.
    // 1. Сравнивается системная площадь помещения с площадью из квартирографии.
    // 2. Сравнивается системная площадь помещения, умноженная на коэффициент из приоритета
    // с площадью из квартирографии.
    public bool CheckActualRoomAreas() {
        foreach(var room in _rooms) {
            if(Math.Abs(room.AreaRevit - room.Area) > _maxAreaDeviation) {
                return false;
            }

            if(Math.Abs(room.AreaCoefRevit - room.AreaCoef) > _maxAreaDeviation) {
                return false;
            }
        }

        return true;
    }
}
