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
    internal class InclinedWidthGetter : IParameterGetter<DoubleParamValue> {
        private readonly MepCurveWallClash _clash;
        private readonly IParameterGetter<DoubleParamValue> _sizeGetter;

        public InclinedWidthGetter(MepCurveWallClash clash, IParameterGetter<DoubleParamValue> sizeGetter) {
            _clash = clash;
            _sizeGetter = sizeGetter;
        }

        public ParameterValuePair<DoubleParamValue> GetParamValue() {
            var width = GetWidth();

            return new ParameterValuePair<DoubleParamValue>() {
                ParamName = RevitRepository.OpeningWidth,
                TValue = new DoubleParamValue(width)
            };
        }

        private double GetWidth() {
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;

            var wallLine = _clash.Wall.GetСentralLine()
                                      .GetTransformedLine(_clash.WallTransform);
            var projector = new PlaneProjector(wallLine.Origin, wallLine.Direction, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var directions = new RoundMepDirsGetter(_clash, projector).GetDirections();
            return new MaxSizeGetter(_clash, projector).GetSize(directions, size);
        }
    }
}
