
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class RoomAreaCalculation : DoubleParamCalculation {
        public RoomAreaCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.RoomArea;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            CalculationValue = ConvertValueToSquareMeters(spatialElement.RoomArea, _accuracy);
        }

        protected override bool CheckSetParamValue(SpatialElementViewModel spatialElement) {
            return true;
        }
    }
}
