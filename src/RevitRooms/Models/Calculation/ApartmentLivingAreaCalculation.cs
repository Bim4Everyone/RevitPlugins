using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class ApartmentLivingAreaCalculation : DoubleParamCalculation {
        public ApartmentLivingAreaCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.ApartmentLivingArea;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(spatialElement.IsRoomLiving == true && spatialElement.Phase.ElementId == Phase.Id) {
                CalculationValue += ConvertValueToSquareMeters(spatialElement.RoomArea, _accuracy);
            }
        }
    }
}
