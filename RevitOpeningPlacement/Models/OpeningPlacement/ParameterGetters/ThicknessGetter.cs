
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class ThicknessGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;

        public ThicknessGetter(MepCurveWallClash clash) {
            _clash = clash;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningThickness,
                TValue = new DoubleParamValue(_clash.Wall.Width)
            };
        }
    }
}
