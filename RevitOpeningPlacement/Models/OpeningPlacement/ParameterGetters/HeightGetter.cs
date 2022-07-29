using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class HeightGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOptions;
        public HeightGetter(MepCurveWallClash clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var height = _clash.Curve.GetHeight();
            height += _categoryOptions.GetOffset(height);

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningHeight,
                TValue = new DoubleParamValue(height)
            };
        }
    }
}
