
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class HeightValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly MEPCurve _curve;
        private readonly MepCategory _categoryOptions;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public HeightValueGetter(MEPCurve curve, MepCategory categoryOptions) {
            _curve = curve;
            _categoryOptions = categoryOptions;
        }

        /// <summary>
        /// Возвращает значение высоты в единицах Revit
        /// </summary>
        /// <returns></returns>
        public DoubleParamValue GetValue() {
            var height = _curve.GetHeight();
            height += _categoryOptions.GetOffsetValue(height);
            var roundHeight = RoundFeetToMillimeters(height, _categoryOptions.Rounding);

            //проверка на недопустимо малые габариты
            if(roundHeight < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }

            return new DoubleParamValue(roundHeight);
        }
    }
}
