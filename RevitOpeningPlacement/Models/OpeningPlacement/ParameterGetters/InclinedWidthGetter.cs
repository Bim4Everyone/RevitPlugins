using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

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
            var angleToY = XYZ.BasisY.AngleOnPlaneTo(transformedMepLine.Direction, XYZ.BasisZ);

            if(Math.Abs(Math.Cos(angleToY)) < 0.0001) {
                return new MaxSizeGetter(_clash).GetSize(XYZ.BasisY, size);
            } else if(Math.Abs(Math.Abs(Math.Cos(angleToY)) - 1) < 0.0001) {
                return new MaxSizeGetter(_clash).GetSize(XYZ.BasisX, size);
            } else {
                var vectorY = (transformedMepLine.Direction.GetLength() / Math.Cos(angleToY)) * XYZ.BasisY;
                var direction = (vectorY - transformedMepLine.Direction).Normalize();

                return new MaxSizeGetter(_clash).GetSize(direction, size);
            }
        }
    }
}
