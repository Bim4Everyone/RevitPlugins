using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitDeclarations.Models
{
    internal class RoomElement {
        private readonly Room _revitRoom;
        private readonly double _areaRevit;
        private readonly double _areaCoefRevit;

        public RoomElement(Room room, DeclarationSettings settings) {
            _revitRoom = room;

            _areaRevit = ParamConverter.ConvertArea(_revitRoom.Area, 2);
            _areaCoefRevit = CalculateAreaCoefRevit(settings);
        }

        public Room RevitRoom => _revitRoom;

        public ElementId RoomLevel => _revitRoom.LevelId;
        public string Name => _revitRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString().ToLower();
        public string NameLower => Name.ToLower();
        public string DeclarationName => $"{Name}_{Number}";
        public double AreaRevit => _areaRevit;
        public double AreaCoefRevit => _areaCoefRevit;
        public string Number => _revitRoom.Number.ToString();


        private double CalculateAreaCoefRevit(DeclarationSettings settings) {
            double areaCoefRevit;
            PrioritiesConfig priorConfig = settings.PrioritiesConfig;

            if(priorConfig.Balcony.CheckName(Name) || priorConfig.Terrace.CheckName(Name)) {
                areaCoefRevit = AreaRevit * 0.3;
            } else if(priorConfig.Loggia.CheckName(Name)) {
                areaCoefRevit = AreaRevit * 0.5;
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
