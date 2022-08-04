using System.Collections.Generic;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRectangleCurveWallParamterGetter : IParametersGetter {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _mepCategory;

        public PerpendicularRectangleCurveWallParamterGetter(MepCurveWallClash clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, new HeightValueGetter(_clash, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new WidthValueGetter(_clash, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new ThicknessValueGetter(_clash)).GetParamValue();
        }
    }
}
