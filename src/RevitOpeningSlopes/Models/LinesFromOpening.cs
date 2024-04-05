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
            XYZ dir1 = geomLine.Direction;
            double x = dir.X;
            double y = dir.Y;
            double z = dir.Z;
            XYZ origin = geomLine.Origin;
            XYZ normal = new XYZ(z - y, x - z, y - x);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketch = SketchPlane.Create(_revitRepository.Document, plane);
            ModelLine line = _revitRepository.Document.Create.NewModelCurve(geomLine, sketch) as ModelLine;
        }
        //public Line CreateLineFromOffsetPoint(FamilyInstance opening) {
        //    XYZ openingOrigin = _revitRepository.GetOpeningOriginBoundingBox(opening);
        //    XYZ openingVector = _revitRepository.GetOpeningVector(opening);
        //    const double frontLineLength = 1500;
        //    const double backwardOffset = 500;
        //    XYZ startPointBbox = new XYZ(openingOrigin.X, openingOrigin.Y, openingOrigin.Z) - openingVector
        //            * _revitRepository.ConvertToFeet(backwardOffset);
        //    XYZ frontOffsetPoint = new XYZ(startPointBbox.X, startPointBbox.Y, startPointBbox.Z)
        //        + openingVector * _revitRepository.ConvertToFeet(frontLineLength);
        //    Line lineFromOffsetPoint = CreateLineFromOpening(
        //        frontOffsetPoint, opening, frontLineLength, DirectionEnum.Back);
        //    return lineFromOffsetPoint;
        //}
        public ICollection<XYZ> SplitCurveToPoints(Curve curve, double step) {
            double curveLength = curve.Length;
            if(step >= curveLength) {
                //Если шаг больше длины линии, то возвращаем середину линии
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
        public Line CreateLineFromOpening(XYZ origin, XYZ openingVector, double length = 1000,
            DirectionEnum direction = DirectionEnum.Right) {

            XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
            length = _revitRepository.ConvertToFeet(length);
            double openingLocationZ = origin.Z;

            XYZ startPoint = new XYZ(origin.X, origin.Y, openingLocationZ);
            XYZ endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) + normalVector
                * length;
            if(direction == DirectionEnum.Left) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) - normalVector
                    * length;
            }
            if(direction == DirectionEnum.Forward) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) + openingVector
                    * length;

            }
            if(direction == DirectionEnum.Back) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ) - openingVector
                    * length;
            }
            if(direction == DirectionEnum.Top) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ + length);
            }
            if(direction == DirectionEnum.Down) {
                endPoint = new XYZ(origin.X, origin.Y, openingLocationZ - length);
            }
            Line line = Line.CreateBound(startPoint, endPoint);

            return line;
        }

    }

}
