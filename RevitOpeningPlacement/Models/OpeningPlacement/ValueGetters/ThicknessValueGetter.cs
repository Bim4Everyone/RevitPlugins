
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class ThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;


        public ThicknessValueGetter(MepCurveWallClash clash) {
            _clash = clash;
        }

        public DoubleParamValue GetValue() {
            return new DoubleParamValue(_clash.Wall.Width);
        }
    }
}
