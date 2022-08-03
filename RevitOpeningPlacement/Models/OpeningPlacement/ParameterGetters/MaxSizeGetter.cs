using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class MaxSizeGetter {
        private readonly MepCurveWallClash _clash;
        private readonly IProjector _projector;

        public MaxSizeGetter(MepCurveWallClash clash, IProjector projector) {
            _clash = clash;
            _projector = projector;
        }

        public double GetSize(XYZ direction, double distance) {
            var transformedMepLine = _clash.GetTransformedMepLine();

            //Наклоненный размер получают следующим образом: осевую линию инженерной системы смещают на заданное значение
            //(равное значению размера, например, диаметра)
            //(в положительную и отрицательную стороны) в заданном направлении
            //Далее находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
            //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет размера.
            return GetSizeFromIntersection(transformedMepLine, direction, distance);
        }

        private double GetSizeFromIntersection(Line mepCurve, XYZ direction, double distance) {
            //получение наружной и внутренней граней
            var faces = _clash.Wall.GetFaces().ToList();

            //смещение осевой линии инженерной системы на заданное расстояние в заданном направлении
            var mepCurvePlus = Line.CreateBound(mepCurve.GetEndPoint(0) + distance * direction, mepCurve.GetEndPoint(1) + distance * direction);
            var mepCurveMinus = Line.CreateBound(mepCurve.GetEndPoint(0) - distance * direction, mepCurve.GetEndPoint(1) - distance * direction);

            //получение пересечений полученных линий и граней стены
            var results = GetIntersectionPoints(new[] { mepCurveMinus, mepCurvePlus }, faces);

            //нахождение среди точек пересечений наиболее удаленных друг от друга и расчет расстояния между ними
            var pointPairs = GetPoints(results.Select(item => _projector.ProjectPoint(item)).ToList());
            var maxDistance = pointPairs.Max(pp => pp.Point1.DistanceTo(pp.Point2));

            //нахождение размера по теореме Пифагора
            return Math.Sqrt(Math.Pow(maxDistance, 2) - Math.Pow(_clash.Wall.Width, 2));
        }

        private static IEnumerable<XYZ> GetIntersectionPoints(IEnumerable<Line> lines, IEnumerable<Face> faces) {
            foreach(var face in faces) {
                foreach(var line in lines) {
                    yield return line.GetIntersectionWithFaceFromPlane(face);
                }
            }
        }

        private static IEnumerable<PointPair> GetPoints(List<XYZ> points) {
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
}
