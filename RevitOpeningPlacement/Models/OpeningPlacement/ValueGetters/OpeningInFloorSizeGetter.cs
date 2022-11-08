
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class OpeningInFloorSizeGetter : IValueGetter<DoubleParamValue> {
        private readonly double _max;
        private readonly double _min;

        public OpeningInFloorSizeGetter(double max, double min) {
            _max = max; 
            _min = min;
        }

        DoubleParamValue IValueGetter<DoubleParamValue>.GetValue() {
            return new DoubleParamValue(_max - _min);
        }
    }
}
