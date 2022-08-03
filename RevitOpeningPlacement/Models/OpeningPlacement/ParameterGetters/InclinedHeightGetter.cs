using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
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
            //получение размера, на который будет смещена осевая линия инженерной системы
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;

            var transformedMepLine = _clash.GetTransformedMepLine();
            var projection = new NormalWallPlaneProjector(_clash.Wall, _clash.WallTransform);
            //Получение угла между проекцией осевой линии инженерной системы на плоскость перпендикулярную стене и осью z
            var angle = projection.GetAngleOnPlaneToAxis(transformedMepLine.Direction);
            if(Math.Abs(Math.Cos(angle)) < 0.0001) {
                return new MaxSizeGetter(_clash, projection).GetSize(XYZ.BasisZ, size);
            } else {
                var projectedDir = projection.ProjectVector(transformedMepLine.Direction);

                var vector = (projectedDir.GetLength() / Math.Cos(angle)) * XYZ.BasisZ;
                var dir = (vector - projectedDir).Normalize();

                return new MaxSizeGetter(_clash, projection).GetSize(dir, size);
            }
        }
    }
}
