
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WidthValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly MEPCurve _curve;
        private readonly MepCategory _categoryOptions;

        public WidthValueGetter(MEPCurve curve, MepCategory categoryOptions) {
            _curve = curve;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var width = _curve.GetWidth();
            width += _categoryOptions.GetOffset(width);
            var roundWidth = RoundFeetToMillimeters(width, _categoryOptions.Rounding);

            return new DoubleParamValue(roundWidth);
        }
    }
}
