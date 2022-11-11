
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class HeightValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MEPCurve _curve;
        private readonly MepCategory _categoryOptions;

        public HeightValueGetter(MEPCurve curve, MepCategory categoryOptions) {
            _curve = curve;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var height = _curve.GetHeight();
            height += _categoryOptions.GetOffset(height);

            return new DoubleParamValue(height);
        }
    }
}
