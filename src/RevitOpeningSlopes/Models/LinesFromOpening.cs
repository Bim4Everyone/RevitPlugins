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
        public Line MergeLines(Line firstLine, Line secondLine) {
            XYZ startPoint = firstLine.GetEndPoint(1);
            XYZ endPoint = secondLine.GetEndPoint(1);
            return Line.CreateBound(startPoint, endPoint);
        }
        //public void CreateLines(IEnumerable<FamilyInstance> windows) {
        //    using(var transaction = _revitRepository.Document.StartTransaction("Построение линии")) {
        //        foreach(FamilyInstance window in windows) {
        //            Line line = CreateLineFromOpening(window);
        //            XYZ origin = _revitRepository.GetOpeningLocation(window);
        //            CreateTestModelLine(line);
        //        }
        //        transaction.Commit();
        //    }
        //}

        public Line CreateLineFromOpening(XYZ origin, FamilyInstance opening, double length = 1000,
            DirectionEnum direction = DirectionEnum.Right) {

            XYZ openingVector = _revitRepository.GetOpeningVector(opening);
            XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
            length = _revitRepository.ConvertToFeet(length);
            double openingLocationZ = origin.Z;
            //double distLeftRight = _revitRepository.ConvertToFeet(2000);
            //double distForwardBack = _revitRepository.ConvertToFeet(600);
            //double distTopDown = _revitRepository.ConvertToFeet(3000);

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
