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
            if(spatialElement.Phase.ElementId == Phase.Id 
                && spatialElement.IsRoomLiving == true) {
                CalculationValue++;
            }
        }

        public override bool SetParamValue(SpatialElementViewModel spatialElement) {
            if(spatialElement.Phase.ElementId == Phase.Id) {
                return base.SetParamValue(spatialElement);
            }
         
            return false;
        }
    }
}
