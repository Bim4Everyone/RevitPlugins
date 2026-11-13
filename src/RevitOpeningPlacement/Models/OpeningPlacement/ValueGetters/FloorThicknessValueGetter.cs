
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class FloorThicknessValueGetter : IValueGetter<DoubleParamValue> {
    private readonly CeilingAndFloor _ceilingAndFloor;

    public FloorThicknessValueGetter(CeilingAndFloor ceilingAndFloor) {
        _ceilingAndFloor = ceilingAndFloor ?? throw new System.ArgumentNullException(nameof(ceilingAndFloor));
    }

    public DoubleParamValue GetValue() {
        double thickness = _ceilingAndFloor.GetThickness();
        //проверка на недопустимо малые габариты
        return thickness < ConstantsProvider.OpeningTaskSizeMinValueFeet
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(thickness);
    }
}
