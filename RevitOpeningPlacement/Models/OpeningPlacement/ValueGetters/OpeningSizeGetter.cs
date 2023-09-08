
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class OpeningSizeGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly double _max;
        private readonly double _min;
        private readonly MepCategory[] _mepCategories;
        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public OpeningSizeGetter(double max, double min, params MepCategory[] mepCategories) {
            _max = max;
            _min = min;
            _mepCategories = mepCategories;
        }

        public DoubleParamValue GetValue() {
            var size = _max - _min;
            int mmRound = 0;
            if(_mepCategories != null && _mepCategories.Length > 0) {
                size += _mepCategories.Max(item => item.GetOffsetValue(_max - _min));
                mmRound = _mepCategories.Max(x => x.Rounding);
            } else {
                mmRound = 10;
            }
            size = RoundToCeilingFeetToMillimeters(size, mmRound);
            //проверка на недопустимо малые габариты
            if(size < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }
            return new DoubleParamValue(size);
        }
    }
}
