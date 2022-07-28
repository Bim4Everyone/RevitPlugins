using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveWallParamterGetter : IParametersGetter {
        private readonly MepCurveWallClash _clash;

        public PerpendicularRoundCurveWallParamterGetter(MepCurveWallClash clash) {
            _clash = clash;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DiameterGetter(_clash).GetParamValue();
            yield return new ThicknessGetter(_clash).GetParamValue();
        }
    }
}
