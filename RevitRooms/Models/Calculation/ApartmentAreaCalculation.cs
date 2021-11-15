
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class ApartmentAreaCalculation : DoubleParamCalculation {
        public ApartmentAreaCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.ApartmentArea;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(spatialElement.Phase.ElementId == Phase.Id) {
                CalculationValue += ConvertValueToSquareMeters(spatialElement.Area, _accuracy);
            }
        }
    }
}
