
using System;

using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class ApartmentAreaRatioCalculation : DoubleParamCalculation {
        public ApartmentAreaRatioCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.ApartmentAreaRatio;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(spatialElement.Phase.ElementId == Phase.Id) {
                CalculationValue += ConvertValueToSquareMeters(spatialElement.AreaWithRatio, _accuracy);
            }
        }
    }
}
