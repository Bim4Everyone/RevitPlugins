
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class AreaWithRatioCalculation : DoubleParamCalculation {
        public AreaWithRatioCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.RoomAreaWithRatio;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            CalculationValue = ConvertValueToSquareMeters(spatialElement.ComputeRoomAreaWithRatio(), _accuracy);
        }

        protected override bool CheckSetParamValue(SpatialElementViewModel spatialElement) {
            return true;
        }
    }
}
