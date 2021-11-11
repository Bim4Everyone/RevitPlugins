
using System;

using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class ApartmentFullAreaCalculation : DoubleParamCalculation {
        public ApartmentFullAreaCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.ApartmentFullArea;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(spatialElement.PhaseName.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase)) {
                CalculationValue += ConvertValueToSquareMeters(spatialElement.Area, _accuracy);
            }
        }

        protected override bool CheckSetParamValue(SpatialElementViewModel spatialElement) {
            return base.CheckSetParamValue(spatialElement) || spatialElement.PhaseName.Equals("Межквартирные перегородки", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
