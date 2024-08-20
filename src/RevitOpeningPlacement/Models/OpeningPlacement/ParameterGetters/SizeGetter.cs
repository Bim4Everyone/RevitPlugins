using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class SizeGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly Plane _plane;

        public SizeGetter(MepCurveClash<Wall> clash, Plane plane) {
            _clash = clash;
            _plane = plane;
        }

        public double GetSizeFromProjection(IEnumerable<XYZ> directions, double distance) {
            var transformedMepLine = _clash.GetTransformedMepLine();

            //Наклоненный размер получают следующим образом: осевую линию инженерной системы смещают на заданное значение
            //(равное значению размера, например, диаметра) в заданных направлениях
            //Далее находятся точки пересечения смещенных линий системы с гранями стены, затем из этих точек выбираются те,
            //которые находятся на максимальном расстоянии друг от друга, далее по теореме Пифагора производится расчет размера.
            return GetSizeFromIntersection(transformedMepLine, directions, distance);
        }

        private double GetSizeFromIntersection(Line mepCurve, IEnumerable<XYZ> directions, double distance) {
            //получение наружной и внутренней граней
            var faces = _clash.Element2.GetSideFaces().ToList();

            //смещение осевой линии инженерной системы на заданное расстояние в заданном направлении
            var lines = directions.Select(item => mepCurve.GetLineWithOffset(item, distance))
                                  .ToList();

            //получение пересечений полученных линий и граней стены
            var results = GetIntersectionPoints(lines, faces);

            //нахождение среди точек пересечений наиболее удаленных друг от друга и расчет расстояния между ними
            var pointPairs = GetPoints(results.Select(item => _plane.ProjectPoint(item)).ToList());
            var maxDistance = pointPairs.Max(pp => pp.Point1.DistanceTo(pp.Point2));

            //нахождение размера по теореме Пифагора
            return Math.Sqrt(Math.Pow(maxDistance, 2) - Math.Pow(_clash.Element2.Width, 2));
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
