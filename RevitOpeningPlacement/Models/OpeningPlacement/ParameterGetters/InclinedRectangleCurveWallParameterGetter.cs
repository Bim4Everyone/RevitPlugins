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
            yield return new InclinedWidthGetter(_clash, new WidthGetter(_clash, _mepCategory)).GetParamValue();
            yield return new InclinedHeightGetter(_clash, new HeightGetter(_clash, _mepCategory)).GetParamValue();
            yield return new ThicknessGetter(_clash).GetParamValue();
        }
    }
}
