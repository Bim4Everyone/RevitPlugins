using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class RoomsCountCalculation : IntParamCalculation {
        public RoomsCountCalculation() {
            RevitParam = SharedParamsConfig.Instance.RoomsCount;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(Contains(spatialElement.Room.Name, "спальня", StringComparison.CurrentCultureIgnoreCase)
                || Contains(spatialElement.Room.Name, "кабинет", StringComparison.CurrentCultureIgnoreCase)
                || (Contains(spatialElement.Room.Name, "гостиная", StringComparison.CurrentCultureIgnoreCase)
                    && !Contains(spatialElement.Room.Name, "кухня-гостиная", StringComparison.CurrentCultureIgnoreCase))) {
                CalculationValue++;
            }
        }

        private static bool Contains(string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
