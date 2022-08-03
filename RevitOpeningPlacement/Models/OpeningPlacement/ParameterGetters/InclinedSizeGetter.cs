using System;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedSizeGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly IParameterGetter<DoubleParamValue> _sizeGetter;
        private readonly IProjector _projector;
        private readonly IDirectionsGetter _directionsGetter;
        private readonly string _paramName;

        public InclinedSizeGetter(MepCurveWallClash clash, IParameterGetter<DoubleParamValue> sizeGetter, IProjector projector, IDirectionsGetter directionsGetter, string paramName) {
            _clash = clash;
            _sizeGetter = sizeGetter;
            _projector = projector;
            _directionsGetter = directionsGetter;
            _paramName = paramName;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var height = GetSize();

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = _paramName,
                TValue = new DoubleParamValue(height)
            };
        }

        private double GetSize() {
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;
            var directions = _directionsGetter.GetDirections();
            return new MaxSizeGetter(_clash, _projector).GetSize(directions, size);
        }
    }
}
