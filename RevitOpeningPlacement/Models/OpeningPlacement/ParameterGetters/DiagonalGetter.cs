using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class DiagonalGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly MepCategory _categoryOptions;

        public DiagonalGetter(MepCurveWallClash clash, MepCategory categoryOptions) {
            _clash = clash;
            _categoryOptions = categoryOptions;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var height = _clash.Curve.GetHeight();
            var width = _clash.Curve.GetWidth();

            height += _categoryOptions.GetOffset(height);
            width += _categoryOptions.GetOffset(width);

            var diagonal = Math.Sqrt(Math.Pow(height, 2) + Math.Pow(width, 2));

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningDiameter,
                TValue = new DoubleParamValue(diagonal)
            };
        }
    }
}
