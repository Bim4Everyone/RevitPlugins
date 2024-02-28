using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using dosymep.Revit;

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

        public void CreateLines(IEnumerable<FamilyInstance> windows) {
            using(var transaction = _revitRepository.Document.StartTransaction("Построение линии")) {
                foreach(FamilyInstance window in windows) {
                    Line line = CreateLineFromOpening(window);
                    XYZ origin = _revitRepository.GetOpeningLocation(window);
                    CreateTestModelLine(line);
                }
                transaction.Commit();
            }
        }
        #region old_create_line
        //public Line CreateLine(FamilyInstance opening, DirectionEnum direction = DirectionEnum.Top) {
        //    int zOffsetConst = 200;
        //    XYZ openingLocation = _revitRepository.GetOpeningLocation(opening);
        //    XYZ openingVector = _revitRepository.GetOpeningVector(opening);
        //    XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
        //    double distConstZ = _revitRepository.ConvertToFeet(zOffsetConst) + openingLocation.Z;
        //    double distConstLeftRight = _revitRepository.ConvertToFeet(800);
        //    double distConstTopBottom = _revitRepository.ConvertToFeet(500);
        //    XYZ start_point = new XYZ(openingLocation.X, openingLocation.Y, distConstZ);
        //    //bool is_right = true;
        //    XYZ end_point = new XYZ(openingLocation.X, openingLocation.Y, distConstZ) + normalVector
        //    * distConstLeftRight;
        //    if(direction == DirectionEnum.Left) {
        //        end_point = new XYZ(openingLocation.X, openingLocation.Y, distConstZ) - normalVector
        //        * distConstLeftRight;
        //        //is_right = false;
        //    }
        //    if(direction == DirectionEnum.Top) {
        //        end_point = new XYZ(openingLocation.X, openingLocation.Y, distConstZ) + openingVector
        //        * distConstTopBottom;

        //    }
        //    if(direction == DirectionEnum.Bottom) {
        //        end_point = new XYZ(openingLocation.X, openingLocation.Y, distConstZ) - openingVector
        //        * distConstTopBottom;
        //        //is_right = false;

        //    }
        //    Line line = Line.CreateBound(start_point, end_point);

        //    return line;
        //}
        #endregion
        public Line CreateLineFromOpening(FamilyInstance opening, DirectionEnum direction = DirectionEnum.Top) {
            XYZ openingLocation = _revitRepository.GetOpeningLocation(opening);
            XYZ openingVector = _revitRepository.GetOpeningVector(opening);
            XYZ normalVector = XYZ.BasisZ.CrossProduct(openingVector).Normalize();
            double openingLocationZ = openingLocation.Z;
            double distLeftRight = _revitRepository.ConvertToFeet(2000);
            double distForwardBack = _revitRepository.ConvertToFeet(600);
            double distTopDown = _revitRepository.ConvertToFeet(3000);
            XYZ start_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ);
            XYZ end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ) + normalVector
                * distLeftRight;
            if(direction == DirectionEnum.Left) {
                end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ) - normalVector
                    * distLeftRight;
            }
            if(direction == DirectionEnum.Forward) {
                end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ) + openingVector
                    * distForwardBack;

            }
            if(direction == DirectionEnum.Back) {
                end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ) - openingVector
                    * distForwardBack;
            }
            if(direction == DirectionEnum.Top) {
                end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ + distTopDown);
            }
            if(direction == DirectionEnum.Down) {
                end_point = new XYZ(openingLocation.X, openingLocation.Y, openingLocationZ - distTopDown);
            }
            Line line = Line.CreateBound(start_point, end_point);

            return line;
        }

    }
    internal enum DirectionEnum {
        Right,
        Left,
        Forward,
        Back,
        Top,
        Down
    }
}
