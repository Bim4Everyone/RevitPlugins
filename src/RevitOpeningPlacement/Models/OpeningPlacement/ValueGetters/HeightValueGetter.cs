
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class HeightValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
    private readonly MEPCurve _curve;
    private readonly MepCategory _categoryOptions;

    public HeightValueGetter(MEPCurve curve, MepCategory categoryOptions) {
        _curve = curve;
        _categoryOptions = categoryOptions;
    }

    /// <summary>
    /// Возвращает значение высоты в единицах Revit
    /// </summary>
    public DoubleParamValue GetValue() {
        double height = _curve.GetHeight();
        height += _categoryOptions.GetOffsetValue(height);
        double roundHeight = RoundFeetToMillimeters(height, _categoryOptions.Rounding);

        //проверка на недопустимо малые габариты
        return roundHeight < ConstantsProvider.OpeningTaskSizeMinValueFeet
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(roundHeight);
    }
}
