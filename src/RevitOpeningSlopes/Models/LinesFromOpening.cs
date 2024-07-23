using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {

    internal class LinesFromOpening {
        private readonly RevitRepository _revitRepository;

        public LinesFromOpening(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        /// <summary>
        /// Функция для создания тестовой линии модели в ревите на основе линии класса Line
        /// </summary>
        /// <param name="geomLine"></param>
        public void CreateTestModelLine(Line geomLine) {
            XYZ dir = geomLine.Direction.Normalize();
            double x = dir.X;
            double y = dir.Y;
            double z = dir.Z;

            XYZ origin = geomLine.Origin;
            XYZ normal = new XYZ(z - y, x - z, y - x);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketch = SketchPlane.Create(_revitRepository.Document, plane);
            _revitRepository.Document.Create.NewModelCurve(geomLine, sketch);
        }

        /// <summary>
        /// Функция делит кривую на точки с указанным шагом и возвращает список из этих точек
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="step"></param>
        /// <returns>Список точек</returns>
        public ICollection<XYZ> SplitCurveToPoints(Curve curve, double step) {
            double curveLength = curve.Length;
            if(step >= curveLength) {
                // Если шаг больше длины линии, то возвращаем середину линии
                return new XYZ[] { curve.Evaluate(0.5, true) };
            } else {
                List<XYZ> points = new List<XYZ>();
                points.Add(curve.GetEndPoint(0));
                for(double lengthOfPiece = step; lengthOfPiece < curveLength; lengthOfPiece += step) {
                    points.Add(curve.Evaluate(lengthOfPiece / curveLength, true));
                }
                return points;
            }
        }

        /// <summary>
        /// Функция создает линию из указанного начала координат окна в указанном направлении
        /// </summary>
        /// <param name="origin">Начало координат</param>
        /// <param name="openingVector">Вектор окна</param>
        /// <param name="length">Длина линии</param>
        /// <param name="direction">Направление линии (Enum)</param>
        /// <returns>Линия, созданная из указанного начала координат</returns>
        public Line CreateLineFromOpening(XYZ origin, XYZ openingVector, double length = 1000,
            Direction direction = Direction.Right) {

            XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
            length = _revitRepository.ConvertToFeet(length);
            double openingLocationZ = origin.Z;

            XYZ startPoint = new XYZ(origin.X, origin.Y, openingLocationZ);
            XYZ endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) + normalVector * length;

            if(direction == Direction.Left) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) - normalVector * length;
            }

            if(direction == Direction.Forward) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) + openingVector * length;
            }

            if(direction == Direction.Backward) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) - openingVector * length;
            }

            if(direction == Direction.Top) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ + length);
            }

            if(direction == Direction.Down) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ - length);
            }

            Line line = Line.CreateBound(startPoint, endPoint);

            return line;
        }
    }
}
