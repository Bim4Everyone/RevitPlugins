using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class RoundCurveWithWallParamterGetter : IParameterGetter {
        private readonly MEPCurve _curve;
        private readonly Wall _wall;
        private readonly Transform _transform;

        public RoundCurveWithWallParamterGetter(MEPCurve curve, Wall wall, Transform transform) {
            _curve = curve;
            _wall = wall;
            _transform = transform;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            if(_curve.IsPerpendicular(_wall)) {
                return GetPerpendicularCurveValues();
            } else {
                return GetInclinedCurveValues();
            }
        }

        private IEnumerable<ParameterValuePair> GetPerpendicularCurveValues() {
            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningDiameter,
                Value = new DoubleParamValue(_curve.GetDiameter())
            };

            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningThickness,
                Value = new DoubleParamValue(_wall.Width)
            };
        }

        private IEnumerable<ParameterValuePair> GetInclinedCurveValues() {
            double diameter = GetInclinedDiameter();

            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningDiameter,
                Value = new DoubleParamValue(diameter)
            };

            yield return new ParameterValuePair() {
                ParamName = RevitRepository.OpeningThickness,
                Value = new DoubleParamValue(_wall.Width)
            };
        }

        private double GetInclinedDiameter() {
            var mepLine = (Line) ((LocationCurve) _curve.Location).Curve;
            var faces = _wall.GetFaces().ToList();

            //примерно на 5 м с обеих сторон удлинена осевая линия инжинерной системы
            var elongatedMepLine = Line.CreateBound(mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                                                    mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

            //трансформация осевой линии инженерной системы в систему координат файла со стеной
            var inversedTransform = _transform.Inverse.Multiply(Transform.Identity);
            var transformedMepCurve = Line.CreateBound(inversedTransform.OfPoint(elongatedMepLine.GetEndPoint(0)),
                                                       inversedTransform.OfPoint(elongatedMepLine.GetEndPoint(1)));

            //Получение диаметров задания на отверстия с учетом наклона систем в горизонтальном и вертикальном направлении и выбор максимального из них.

            //Диаметры получаются следующим образом: осевую линию инженерной системы смещают на радиус (в положительную и отрицательную стороны)
            //в плоскостях yOz (для получения вертикального диаметра) и в xOy (для получения горизонтального диаметра).
            //Далее для каждой плоскости находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
            //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет диаметра.
            var diameter = new[] {
                _curve.GetDiameter(),
                GetVerticalDiameter(transformedMepCurve, faces),
                GetHorizontalDiameter(transformedMepCurve, faces),
            }.Max();
            return diameter;
        }

        private double GetVerticalDiameter(Line mepCurve, IEnumerable<Face> faces) {
            //Получение угла между проекцией осевой линии инженерной системы на плоскость yOz и осью z
            var angleToZ = XYZ.BasisZ.AngleOnPlaneTo(mepCurve.Direction, XYZ.BasisX);

            if(Math.Abs(Math.Cos(angleToZ)) < 0.0001) {
                //если угол равен 90 градусов, значит система расположена горизонтально и ее можно смещать строго вверх
                return GetDiameter(mepCurve, faces, XYZ.BasisZ);
            } else {
                //получение вектора, направленного вдоль оси Z, длина которого равной гипотенузе в прямоугольном треугольнике,
                //катетами которого являются проекция осевой линии инженерной системы и направление для описанного выше смещения 
                var vectorZ = (mepCurve.Direction.GetLength() / Math.Cos(angleToZ)) * XYZ.BasisZ;

                //получение смещения путем векторного вычитания
                var direction = (vectorZ - mepCurve.Direction).Normalize();

                return GetDiameter(mepCurve, faces, direction);
            }
        }


        private double GetHorizontalDiameter(Line mepCurve, IEnumerable<Face> faces) {
            //алгорится аналогичен тому, который описан в методе GetVerticalDiameter
            var angleToY = XYZ.BasisY.AngleOnPlaneTo(mepCurve.Direction, XYZ.BasisZ);
            if(Math.Abs(Math.Cos(angleToY)) < 0.0001) {
                return GetDiameter(mepCurve, faces, XYZ.BasisY);
            } else if(Math.Abs(Math.Abs(Math.Cos(angleToY)) - 1) < 0.0001) {
                return GetDiameter(mepCurve, faces, XYZ.BasisX);
            } else {
                var vectorY = mepCurve.Direction.GetLength() / Math.Cos(angleToY) * XYZ.BasisY;
                var direction = (vectorY - mepCurve.Direction).Normalize();

                return GetDiameter(mepCurve, faces, direction);
            }
        }

        private double GetDiameter(Line mepCurve, IEnumerable<Face> faces, XYZ direction) {
            var diameter = _curve.GetDiameter();

            //смещение осевой линии инженерной системы на радиус в направлении, найденном ранее
            var mepCurvePlus = Line.CreateBound(mepCurve.GetEndPoint(0) + diameter / 2 * direction, mepCurve.GetEndPoint(1) + diameter / 2 * direction);
            var mepCurveMinus = Line.CreateBound(mepCurve.GetEndPoint(0) - diameter / 2 * direction, mepCurve.GetEndPoint(1) - diameter / 2 * direction);

            //получение пересечений полученных линий и граней стены
            var results = GetIntersectionPoints(new[] { mepCurveMinus, mepCurvePlus }, faces);

            //нахождение среди точек пересечений наиболее удаленных друг от друга и расчет расстояния между ними
            var pointPairs = GetPoints(results.ToList());
            var maxDistance = pointPairs.Max(pp => pp.Point1.DistanceTo(pp.Point2));

            //нахождение диаметра по теореме Пифагора
            return Math.Sqrt(Math.Pow(maxDistance, 2) - Math.Pow(_wall.Width, 2));
        }

        private IEnumerable<XYZ> GetIntersectionPoints(IEnumerable<Line> lines, IEnumerable<Face> faces) {
            foreach(var face in faces) {
                foreach(var line in lines) {
                    XYZ point;
                    try {
                        point = line.GetIntersectionWithFace(face);
                    } catch {
                        continue; //Возможно, если сформированная линия проходит вне границы существующей грани
                    }
                    yield return point;
                }
            }
        }

        private IEnumerable<PointPair> GetPoints(List<XYZ> points) {
            while(points.Count > 0) {
                var point = points[0];
                points.RemoveAt(0);
                foreach(var p in points) {
                    yield return new PointPair() {
                        Point1 = point,
                        Point2 = p
                    };
                }
            }
        }
    }

    internal class PointPair {
        public XYZ Point1 { get; set; }
        public XYZ Point2 { get; set; }
    }
}
