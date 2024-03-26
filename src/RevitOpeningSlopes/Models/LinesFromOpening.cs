using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models.Enums;

namespace RevitOpeningSlopes.Models {
    [Transaction(TransactionMode.Manual)]

    internal class LinesFromOpening {

        private readonly RevitRepository _revitRepository;


        public LinesFromOpening(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void CreateTestModelLine(Line geomLine) {
            XYZ dir = geomLine.Direction.Normalize();
            double x = dir.X;
            double y = dir.Y;
            double z = dir.Z;
            XYZ origin = geomLine.Origin;
            XYZ normal = new XYZ(z - y, x - z, y - x);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketch = SketchPlane.Create(_revitRepository.Document, plane);
            ModelLine line = _revitRepository.Document.Create.NewModelCurve(geomLine, sketch) as ModelLine;
        }

        public Line CreateLineFromOffsetPoint(FamilyInstance opening) {
            XYZ openingOrigin = _revitRepository.GetOpeningLocation(opening);
            XYZ openingVector = _revitRepository.GetOpeningVector(opening);
            const double offset = 300;
            const double frontLineLength = 900;
            XYZ frontOffsetPoint = new XYZ(openingOrigin.X, openingOrigin.Y, openingOrigin.Z
                + _revitRepository.ConvertToFeet(offset))
                + openingVector * _revitRepository.ConvertToFeet(frontLineLength);
            Line lineFromOffsetPoint = CreateLineFromOpening(
                frontOffsetPoint, opening, frontLineLength, DirectionEnum.Back);
            return lineFromOffsetPoint;
        }
        public ICollection<XYZ> SplitCurveToPoints(Curve curve, double step) {
            double curveLength = curve.Length;
            if(step >= curveLength) {
                //Если шаг больше длины линии, то возвращаем середину линии
                return new XYZ[] { curve.Evaluate(0.5, true) };
            } else {
                List<XYZ> points = new List<XYZ>();
                for(double lengthOfPiece = step; lengthOfPiece < curveLength; lengthOfPiece += step) {
                    points.Add(curve.Evaluate(lengthOfPiece / curveLength, true));
                }
                return points;
            }
        }
        public Line CreateLineFromOpening(XYZ origin, FamilyInstance opening, double length = 1000,
            DirectionEnum direction = DirectionEnum.Right) {

            XYZ openingVector = _revitRepository.GetOpeningVector(opening);
            XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
            length = _revitRepository.ConvertToFeet(length);
            double openingLocationZ = origin.Z;

            XYZ start_point = new XYZ(origin.X, origin.Y, openingLocationZ);
            XYZ end_point = new XYZ(origin.X, origin.Y, openingLocationZ) + normalVector
                * length;
            if(direction == DirectionEnum.Left) {
                end_point = new XYZ(origin.X, origin.Y, openingLocationZ) - normalVector
                    * length;
            }
            if(direction == DirectionEnum.Forward) {
                end_point = new XYZ(origin.X, origin.Y, openingLocationZ) + openingVector
                    * length;

            }
            if(direction == DirectionEnum.Back) {
                end_point = new XYZ(origin.X, origin.Y, openingLocationZ) - openingVector
                    * length;
            }
            if(direction == DirectionEnum.Top) {
                end_point = new XYZ(origin.X, origin.Y, openingLocationZ + length);
            }
            if(direction == DirectionEnum.Down) {
                end_point = new XYZ(origin.X, origin.Y, openingLocationZ - length);
            }
            Line line = Line.CreateBound(start_point, end_point);

            return line;
        }

    }

}
