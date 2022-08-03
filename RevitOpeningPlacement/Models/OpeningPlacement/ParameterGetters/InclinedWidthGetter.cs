using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
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
            var transformedMepLine = _clash.GetTransformedMepLine();

            //получение размера, на который будет смещена осевая линия инженерной системы
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;

            //алгоритм аналогичен тому, который описан в InclinedHeightGetter

            var wallLine = _clash.Wall.GetСentralLine()
                                      .GetTransformedLine(_clash.WallTransform);
            var projector = new PlaneProjector(wallLine.Origin, wallLine.Direction, _clash.WallTransform.OfVector(_clash.Wall.Orientation));
            var angle = projector.GetAngleOnPlaneToAxis(transformedMepLine.Direction);

            if(Math.Abs(Math.Cos(angle)) < 0.0001) {
                return new MaxSizeGetter(_clash, projector).GetSize(projector.GetPlaneY(), size);
            } else {
                var projectedDir = projector.ProjectVector(transformedMepLine.Direction);

                var vector = (projectedDir.GetLength() / Math.Cos(angle)) * projector.GetPlaneY();
                var dir = (vector - projectedDir).Normalize();

                return new MaxSizeGetter(_clash, projector).GetSize(dir, size);
            }
        }
    }
}
