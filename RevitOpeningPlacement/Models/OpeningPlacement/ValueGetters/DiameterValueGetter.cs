
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DiameterValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly MEPCurve _curve;
        private readonly MepCategory _categoryOptions;

        public DiameterValueGetter(MEPCurve curve, MepCategory categoryOptions) {
            _curve = curve;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var diameter = _curve.GetDiameter();
            diameter += _categoryOptions.GetOffset(diameter);
            var roundDiameter = RoundFeetToMillimeters(diameter, _categoryOptions.Rounding);

            return new DoubleParamValue(roundDiameter);
        }
    }
}
