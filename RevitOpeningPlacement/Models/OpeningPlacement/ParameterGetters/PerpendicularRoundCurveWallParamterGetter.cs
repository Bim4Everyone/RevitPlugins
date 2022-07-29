using System.Collections.Generic;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRoundCurveWallParamterGetter : IParametersGetter {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;

        public PerpendicularRoundCurveWallParamterGetter(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DiameterGetter(_clash, _mepCategory).GetParamValue();
            yield return new ThicknessGetter(_clash).GetParamValue();
        }
    }
}
