using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models
{
    internal class RoomElement {
        private readonly Room _revitRoom;
        private readonly string _name;

        private readonly double _areaRevit;
        private readonly double _areaCoefRevit;
        private readonly double _areaLivingRevit;
        private readonly double _areaNonSummerRevit;
        private readonly double _area;
        private readonly double _areaCoef;

        public RoomElement(Room room, DeclarationSettings settings) {
            _revitRoom = room;

            _name = _revitRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

            _areaRevit = ParamConverter.ConvertArea(_revitRoom.Area, settings.AccuracyForArea);
            RoomAreaCalculator areaCalculator = new RoomAreaCalculator(settings, this);
            _areaCoefRevit = areaCalculator.CalculateAreaCoefRevit();
            _areaLivingRevit = areaCalculator.CalculateAreaLivingRevit();
            _areaNonSummerRevit = areaCalculator.CalculateAreaNonSummerRevit();

            _area = GetAreaParamValue(settings.RoomAreaParam, settings.AccuracyForArea);
            _areaCoef = GetAreaParamValue(settings.RoomAreaCoefParam, settings.AccuracyForArea);
        }

        [JsonIgnore] 
        public Room RevitRoom => _revitRoom;

        [JsonIgnore]
        public ElementId RoomLevel => _revitRoom.LevelId;
        [JsonProperty("room_type")]
        public string Name => _name;
        [JsonIgnore]
        public string DeclarationName => $"{Name}_{Number}";

        [JsonIgnore]
        public double AreaRevit => _areaRevit;
        [JsonIgnore]
        public double AreaCoefRevit => _areaCoefRevit;
        [JsonIgnore]
        public double AreaLivingRevit => _areaLivingRevit;
        [JsonIgnore]
        public double AreaNonSummerRevit => _areaNonSummerRevit;

        [JsonProperty("area")]
        public double Area => _area;
        [JsonProperty("area_k")]
        public double AreaCoef => _areaCoef;
        [JsonProperty("number")]
        public string Number => _revitRoom.Number;

        public string GetTextParamValue(Parameter parameter) {
            return RevitRoom.GetParamValueOrDefault<string>(parameter.Definition.Name);
        }

        public double GetAreaParamValue(Parameter parameter, int accuracy) {
            var value = RevitRoom.GetParamValueOrDefault<double>(parameter.Definition.Name);
            return ParamConverter.ConvertArea(value, accuracy);
        }

        public double GetLengthParamValue(Parameter parameter, int accuracy) {
            var value = RevitRoom.GetParamValueOrDefault<double>(parameter.Definition.Name);
            return ParamConverter.ConvertLength(value, accuracy);
        }

        public int GetIntParamValue(Parameter parameter) {
            return RevitRoom.GetParamValueOrDefault<int>(parameter.Definition.Name);
        }
    }
}
