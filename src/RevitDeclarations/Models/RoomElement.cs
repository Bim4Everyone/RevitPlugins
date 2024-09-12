using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

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

            _areaRevit = ParamConverter.ConvertArea(_revitRoom.Area, settings.Accuracy);
            RoomAreaCalculator areaCalculator = new RoomAreaCalculator(settings, this);
            _areaCoefRevit = areaCalculator.CalculateAreaCoefRevit();
            _areaLivingRevit = areaCalculator.CalculateAreaLivingRevit();
            _areaNonSummerRevit = areaCalculator.CalculateAreaNonSummerRevit();

            _area = GetAreaParamValue(settings.RoomAreaParam, settings.Accuracy);
            _areaCoef = GetAreaParamValue(settings.RoomAreaCoefParam, settings.Accuracy);
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
            return RevitRoom.LookupParameter(parameter.Definition.Name).AsString();
        }

        public double GetAreaParamValue(Parameter parameter, int accuracy) {
            double value = RevitRoom.LookupParameter(parameter.Definition.Name).AsDouble();
            return ParamConverter.ConvertArea(value, accuracy);
        }

        public double GetLengthParamValue(Parameter parameter, int accuracy) {
            double value = RevitRoom.LookupParameter(parameter.Definition.Name).AsDouble();
            return ParamConverter.ConvertLength(value, accuracy);
        }

        public int GetIntParamValue(Parameter parameter) {
            return RevitRoom.LookupParameter(parameter.Definition.Name).AsInteger();
        }
    }
}
