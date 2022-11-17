
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WallThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;

        public WallThicknessValueGetter(MepCurveClash<Wall> clash) {
            _clash = clash;
        }

        public DoubleParamValue GetValue() {
            return new DoubleParamValue(_clash.Element2.Width);
        }
    }
}
