using System.Collections.Generic;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedRectangleCurveWallParameterGetter : IParametersGetter {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;

        public InclinedRectangleCurveWallParameterGetter(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepHeightGetter().GetParamValue();
            yield return new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepWidthGetter().GetParamValue();
            yield return new ThicknessGetter(_clash).GetParamValue();
        }
    }
}
