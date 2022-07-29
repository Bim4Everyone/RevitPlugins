using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class WidthGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOptions;

        public WidthGetter(MepCurveWallClash clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var width = _clash.Curve.GetWidth();
            width += _categoryOptions.GetOffset(width);

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningWidth,
                TValue = new DoubleParamValue(width)
            };
        }
    }
}
