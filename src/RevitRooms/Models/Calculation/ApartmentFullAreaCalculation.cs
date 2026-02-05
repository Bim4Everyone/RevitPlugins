using System;

using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation;
internal class ApartmentFullAreaCalculation : DoubleParamCalculation {
    private readonly string _phaseNameCheck;

    public ApartmentFullAreaCalculation(int percent, int accuracy, string phaseName)
        : base(percent, accuracy) {
        RevitParam = SharedParamsConfig.Instance.ApartmentFullArea;
        _phaseNameCheck = phaseName;
    }

    public override void CalculateParam(SpatialElementViewModel spatialElement) {
        if(spatialElement.PhaseName.Equals(_phaseNameCheck, StringComparison.CurrentCultureIgnoreCase)) {
            CalculationValue += ConvertValueToSquareMeters(spatialElement.Area, _accuracy);
        }
    }

    protected override bool CheckSetParamValue(SpatialElementViewModel spatialElement) {
        return base.CheckSetParamValue(spatialElement) 
            || spatialElement.PhaseName.Equals(_phaseNameCheck, StringComparison.CurrentCultureIgnoreCase);
    }
}
