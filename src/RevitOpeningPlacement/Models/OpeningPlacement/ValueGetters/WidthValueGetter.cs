
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class WidthValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
    private readonly MEPCurve _curve;
    private readonly MepCategory _categoryOptions;

    public WidthValueGetter(MEPCurve curve, MepCategory categoryOptions) {
        _curve = curve;
        _categoryOptions = categoryOptions;
    }

    public DoubleParamValue GetValue() {
        double width = _curve.GetWidth();
        width += _categoryOptions.GetOffsetValue(width);
        double roundWidth = RoundFeetToMillimeters(width, _categoryOptions.Rounding);

        //проверка на недопустимо малые габариты
        return roundWidth < ConstantsProvider.OpeningTaskSizeMinValueFeet
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(roundWidth);
    }
}
