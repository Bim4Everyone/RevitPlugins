
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class WallThicknessValueGetter : IValueGetter<DoubleParamValue> {
    private readonly Wall _wall;

    public WallThicknessValueGetter(Wall wall) {
        _wall = wall ?? throw new System.ArgumentNullException(nameof(wall));
    }

    public DoubleParamValue GetValue() {
        //проверка на недопустимо малые габариты
        return _wall.Width < ConstantsProvider.OpeningTaskSizeMinValueFeet
            ? throw new SizeTooSmallException("Заданный габарит отверстия слишком мал")
            : new DoubleParamValue(_wall.Width);
    }
}
