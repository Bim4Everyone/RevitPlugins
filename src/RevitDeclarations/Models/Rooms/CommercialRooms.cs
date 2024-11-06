using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class CommercialRooms : RoomGroup {
        private readonly CommercialSettings _settings;

        public CommercialRooms(IEnumerable<RoomElement> rooms, DeclarationSettings settings)
            : base(rooms, settings) {
            _settings = (CommercialSettings) settings;
        }

        [JsonProperty("building_number")]
        public string BuildingNumber => _firstRoom.GetTextParamValue(_settings.BuildingNumberParam);
        [JsonProperty("construction_works")]
        public string ConstrWorksNumber => _firstRoom.GetTextParamValue(_settings.ConstrWorksNumberParam);
        [JsonProperty("ceiling_height")]
        public double RoomsHeight => _firstRoom.GetLengthParamValue(_settings.RoomsHeightParam, _accuracy);

        [JsonProperty("type")]
        public override string Department {
            get {
                if(string.IsNullOrEmpty(_firstRoom.GetTextParamValue(_settings.MultiStoreyParam))) {
                    return _firstRoom.GetTextParamValue(_settings.DepartmentParam);
                } else {
                    return "Коммерция на двух и более этажах";
                }
            }
        }

        public override string Number {
            get {
                if(_settings.AddPrefixToNumber) {
                    return $"{base.Number}-{_firstRoom.Number}";
                } else {
                    return base.Number;
                }
            }
        }

        public override double AreaMain {
            get {
                if(_isOneRoomGroup) {
                    return _firstRoom.Area;
                } else {
                    return AreaMain;
                }
            }
        }

        public string GroupName {
            get {
                if(_isOneRoomGroup) {
                    return _firstRoom.Name;
                } else {
                    return _firstRoom.GetTextParamValue(_settings.GroupNameParam);
                }
            }
        }
    }
}
