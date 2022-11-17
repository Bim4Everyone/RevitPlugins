
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class FloorThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<CeilingAndFloor> _clash;

        public FloorThicknessValueGetter(MepCurveClash<CeilingAndFloor> clash) {
            _clash = clash;
        }

        DoubleParamValue IValueGetter<DoubleParamValue>.GetValue() {
            return new DoubleParamValue(_clash.Element2.GetThickness());
        }
    }
}
