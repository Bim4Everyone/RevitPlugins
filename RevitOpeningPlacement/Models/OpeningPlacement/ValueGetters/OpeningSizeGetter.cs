
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class OpeningSizeGetter : IValueGetter<DoubleParamValue> {
        private readonly double _max;
        private readonly double _min;
        private readonly MepCategory[] _mepCategories;

        public OpeningSizeGetter(double max, double min, params MepCategory[] mepCategories) {
            _max = max;
            _min = min;
            _mepCategories = mepCategories;
        }

        DoubleParamValue IValueGetter<DoubleParamValue>.GetValue() {
            if(_mepCategories != null && _mepCategories.Length > 0) {
                return new DoubleParamValue(_max - _min + _mepCategories.Max(item => item.GetOffset(_max - _min)));
            }
            return new DoubleParamValue(_max - _min);
        }
    }
}
