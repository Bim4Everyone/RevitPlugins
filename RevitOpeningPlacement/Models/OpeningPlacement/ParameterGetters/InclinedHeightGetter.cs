using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.DirGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.Projectors;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedHeightGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly IParameterGetter<DoubleParamValue> _sizeGetter;

        public InclinedHeightGetter(MepCurveWallClash clash, IParameterGetter<DoubleParamValue> sizeGetter) {
            _clash = clash;
            _sizeGetter = sizeGetter;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var height = GetHeight();

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningHeight,
                TValue = new DoubleParamValue(height)
            };
        }
        private double GetHeight() {
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;

            var wallLine = _clash.Wall.GetСentralLine()
                                      .GetTransformedLine(_clash.WallTransform);
            var projector = new PlaneProjector(wallLine.Origin, XYZ.BasisZ, _clash.WallTransform.OfVector(_clash.Wall.Orientation));

            var directions = new RoundMepDirsGetter(_clash, projector).GetDirections();
            return new MaxSizeGetter(_clash, projector).GetSize(directions, size);
        }
    }
}
