using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models
{
    internal class RoomElement {
        private readonly Room _revitRoom;
        private readonly double _areaRevit;
        private readonly double _areaCoefRevit;
        private readonly double _area;
        private readonly double _areaCoef;

        public RoomElement(Room room, DeclarationSettings settings) {
            _revitRoom = room;

            _areaRevit = ParamConverter.ConvertArea(_revitRoom.Area, settings.Accuracy);
            _areaCoefRevit = CalculateAreaCoefRevit(settings);
            _area = GetAreaParamValue(settings.RoomAreaParam, settings.Accuracy);
            _areaCoef = GetAreaParamValue(settings.RoomAreaCoefParam, settings.Accuracy);
        }

        [JsonIgnore] 
        public Room RevitRoom => _revitRoom;

        [JsonIgnore]
        public ElementId RoomLevel => _revitRoom.LevelId;
        [JsonProperty("room_type")]
        public string Name => _revitRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
        [JsonIgnore]
        public string NameLower => Name.ToLower();
        [JsonIgnore]
        public string DeclarationName => $"{Name}_{Number}";

        [JsonIgnore]
        public double AreaRevit => _areaRevit;
        [JsonIgnore]
        public double AreaCoefRevit => _areaCoefRevit;
        [JsonProperty("area")]
        public double Area => _area;
        [JsonProperty("area_k")]
        public double AreaCoef => _areaCoef;
        [JsonProperty("number")]
        public string Number => _revitRoom.Number.ToString();


        private double CalculateAreaCoefRevit(DeclarationSettings settings) {
            double areaCoefRevit;
            PrioritiesConfig priorConfig = settings.PrioritiesConfig;

            if(priorConfig.Balcony.CheckName(Name) || priorConfig.Terrace.CheckName(Name)) {
                areaCoefRevit = AreaRevit * priorConfig.Balcony.AreaCoefficient;
            } else if(priorConfig.Loggia.CheckName(Name)) {
                areaCoefRevit = AreaRevit * priorConfig.Loggia.AreaCoefficient;
            } else {
                areaCoefRevit = AreaRevit;
            }

            return areaCoefRevit;
        }

        public bool HasParameter(Parameter parameter) {
            if(RevitRoom.LookupParameter(parameter.Definition.Name) == null) {
                return false;
            }
            return true;
        }

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
