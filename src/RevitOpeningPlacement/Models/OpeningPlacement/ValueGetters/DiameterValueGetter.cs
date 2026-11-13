
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class DiameterValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
    private readonly MEPCurve _curve;
    private readonly MepCategory _categoryOptions;

    public DiameterValueGetter(MEPCurve curve, MepCategory categoryOptions) {
        _curve = curve;
        _categoryOptions = categoryOptions;
    }

    public DoubleParamValue GetValue() {
        double diameter = _curve.GetDiameter();
        diameter += _categoryOptions.GetOffsetValue(diameter);
        double roundDiameter = RoundFeetToMillimeters(diameter, _categoryOptions.Rounding);

        //проверка на недопустимо малые габариты
        return roundDiameter < ConstantsProvider.OpeningTaskSizeMinValueFeet
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(roundDiameter);
    }
}
