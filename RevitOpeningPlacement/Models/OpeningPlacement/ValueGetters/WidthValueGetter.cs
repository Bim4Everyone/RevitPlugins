
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WidthValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOptions;

        public WidthValueGetter(MepCurveWallClash clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var width = _clash.Curve.GetWidth();
            width += _categoryOptions.GetOffset(width);

            return new DoubleParamValue(width);
        }
    }
}
