using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class RoomGroup {
        private protected const double _maxAreaDeviation = 0.2;

        private protected readonly StringComparer _strComparer = StringComparer.OrdinalIgnoreCase;

        private readonly DeclarationSettings _settings;
        private protected readonly RoomParamProvider _paramProvider;
        private protected readonly int _accuracy;

        private protected readonly IEnumerable<RoomElement> _rooms;

        private protected readonly RoomElement _firstRoom;
        private protected readonly bool _isOneRoomGroup = false;

        public RoomGroup(IEnumerable<RoomElement> rooms, DeclarationSettings settings, RoomParamProvider paramProvider) {
            _settings = settings;
            _paramProvider = paramProvider;
            _accuracy = settings.Accuracy;

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
        public virtual double AreaMain => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracy);

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
}
