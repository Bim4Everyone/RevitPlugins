
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class OpeningInFloorSizeGetter : IValueGetter<DoubleParamValue> {
        private readonly double _max;
        private readonly double _min;
        private readonly MepCategory _mepCategory;

        public OpeningInFloorSizeGetter(double max, double min, MepCategory mepCategory = null) {
            _max = max;
            _min = min;
            _mepCategory = mepCategory;
        }

        DoubleParamValue IValueGetter<DoubleParamValue>.GetValue() {
            if(_mepCategory != null) {
                return new DoubleParamValue(_max - _min + _mepCategory.GetOffset(_max - _min));
            }
            return new DoubleParamValue(_max - _min);
        }
    }
}
