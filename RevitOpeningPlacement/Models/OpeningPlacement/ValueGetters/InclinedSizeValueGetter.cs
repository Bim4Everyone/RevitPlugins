
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class InclinedSizeValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly Plane _plane;
        private readonly IValueGetter<DoubleParamValue> _sizeValueGetter;
        private readonly IDirectionsGetter _directionsGetter;

        public InclinedSizeValueGetter(MepCurveWallClash clash, IValueGetter<DoubleParamValue> sizeValueGetter, Plane plane, IDirectionsGetter directionsGetter) {
            _clash = clash;
            _sizeValueGetter = sizeValueGetter;
            _plane = plane;
            _directionsGetter = directionsGetter;
        }

        public DoubleParamValue GetValue() {
            var size = GetSize();
            return new DoubleParamValue(size);
        }

        private double GetSize() {
            //получение длины размера, спроецированного на плоскость
            var size = _sizeValueGetter.GetValue().TValue / 2;
            var directions = _directionsGetter.GetDirectionsOnPlane(_plane);
            return new SizeGetter(_clash, _plane).GetSizeFromProjection(directions, size);
        }
    }
}
