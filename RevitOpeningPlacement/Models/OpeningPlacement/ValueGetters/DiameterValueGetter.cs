
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DiameterValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _categoryOptions;

        public DiameterValueGetter(MepCurveClash<Wall> clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var diameter = _clash.Curve.GetDiameter();
            diameter += _categoryOptions.GetOffset(diameter);

            return new DoubleParamValue(diameter);
        }
    }
}
