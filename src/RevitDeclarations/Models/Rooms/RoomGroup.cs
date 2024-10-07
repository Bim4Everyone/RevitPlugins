using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class RoomGroup {
        private protected const double _maxAreaDeviation = 0.2;

        private protected readonly StringComparer _strComparer = StringComparer.OrdinalIgnoreCase;

        private protected readonly DeclarationSettings _settings;
        private protected readonly int _accuracy;

        private protected readonly IEnumerable<RoomElement> _rooms;

        private protected readonly RoomElement _firstRoom;

        public RoomGroup(IEnumerable<RoomElement> rooms, DeclarationSettings settings) {
            _settings = settings;
            _accuracy = settings.Accuracy;

            _rooms = rooms.ToList();
            _firstRoom = rooms.FirstOrDefault();
        }

        [JsonProperty("rooms")]
        public IEnumerable<RoomElement> Rooms => _rooms;

        [JsonProperty("full_number")]
        public string FullNumber => _firstRoom.GetTextParamValue(_settings.ApartmentFullNumberParam);
        [JsonProperty("type")]
        public string Department {
            get {
                if(string.IsNullOrEmpty(_firstRoom.GetTextParamValue(_settings.MultiStoreyParam))) {
                    return _firstRoom.GetTextParamValue(_settings.DepartmentParam);
                } else {
                    return "Квартира на двух и более этажах";
                }
            }
        }
        [JsonProperty("floor_number")]
        public string Level {
            get {
                var levelNames = _rooms
                    .Select(x => x.GetTextParamValue(_settings.LevelParam))
                    .Distinct();
                return string.Join(",", levelNames);
            }
        }
        [JsonProperty("section")]
        public string Section => _firstRoom.GetTextParamValue(_settings.SectionParam);
        [JsonProperty("building")]
        public string Building => _firstRoom.GetTextParamValue(_settings.BuildingParam);
        [JsonProperty("number")]
        public string Number => _firstRoom.GetTextParamValue(_settings.ApartmentNumberParam);
        [JsonProperty("area")]
        public double AreaMain => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracy);
    }
}
