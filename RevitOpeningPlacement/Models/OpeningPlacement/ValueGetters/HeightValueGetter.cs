
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class HeightValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _categoryOptions;

        public HeightValueGetter(MepCurveClash<Wall> clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var height = _clash.Curve.GetHeight();
            height += _categoryOptions.GetOffset(height);

            return new DoubleParamValue(height);
        }
    }
}
