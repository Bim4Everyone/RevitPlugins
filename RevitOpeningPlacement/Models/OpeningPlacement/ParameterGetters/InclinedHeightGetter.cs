using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

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
            var transformedMepLine = _clash.GetTransformedMepLine();

            //получение размера, на который будет смещена осевая линия инженерной системы
            var size = _sizeGetter.GetParamValue().TValue.TValue / 2;

            //Получение угла между проекцией осевой линии инженерной системы на плоскость yOz и осью z
            var angleToZ = XYZ.BasisZ.AngleOnPlaneTo(transformedMepLine.Direction, XYZ.BasisX);

            if(Math.Abs(Math.Cos(angleToZ)) < 0.0001) {
                //если угол равен 90 градусов, значит система расположена горизонтально и ее можно смещать строго вверх
                return new MaxSizeGetter(_clash).GetSize(XYZ.BasisZ, size);
            } else {
                //получение вектора, направленного вдоль оси Z, длина которого равной гипотенузе в прямоугольном треугольнике,
                //катетами которого являются проекция осевой линии инженерной системы и направление для описанного выше смещения 
                var vectorZ = (transformedMepLine.Direction.GetLength() / Math.Cos(angleToZ)) * XYZ.BasisZ;

                //получение смещения путем векторного вычитания
                var direction = (vectorZ - transformedMepLine.Direction).Normalize();

                return new MaxSizeGetter(_clash).GetSize(direction, size);
            }
        }
    }
}
