using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class WallExtension {
        public static Line GetLine(this Wall wall) {
            return (Line) ((LocationCurve) wall.Location).Curve;
        }

        public static Line GetСentralLine(this Wall wall) {
            var centralPoint = GetCentralPoint(wall);

            var wallLine = wall.GetLine();

            var bottomCentralPoint = new XYZ(centralPoint.X, centralPoint.Y, wallLine.GetEndPoint(0).Z);

            return Line.CreateBound(bottomCentralPoint + wallLine.Direction * wallLine.Length / 2,
                                    bottomCentralPoint - wallLine.Direction * wallLine.Length / 2);
        }

        public static Plane GetVerticalNormalPlane(this Wall wall) {
            var wallLine = wall.GetСentralLine();
            return Plane.CreateByOriginAndBasis(wallLine.Origin, wall.Orientation, XYZ.BasisZ);
        }

        public static Plane GetHorizontalNormalPlane(this Wall wall) {
            var wallLine = wall.GetСentralLine();
            return Plane.CreateByOriginAndBasis(wallLine.Origin, wall.Orientation, wallLine.Direction);
        }

        private static XYZ GetCentralPoint(Wall wall) {
            var bb = wall.GetBoundingBox();
            XYZ bbCenter = bb.Min + (bb.Max - bb.Min) / 2;
            //если стена соединена с другой стеной, то торец в месте соединения образует острый угол,
            //из-за чего центр бокса - это не геометрический центр стены.

            PlanarFace exteriorFace = wall.GetGeometryObjectFromReference(
                HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior)[0]) as PlanarFace;
            XYZ alignVector;
            if(exteriorFace != null) {

                Plane exteriorPlane = Plane.CreateByNormalAndOrigin(exteriorFace.FaceNormal, exteriorFace.Origin);

                XYZ bbCenterProjection = exteriorPlane.ProjectPoint(bbCenter);
                XYZ bbCenterToProjectionVector = bbCenterProjection - bbCenter;

                //получение вектора, который надо прибавить к центру бокса, чтобы получившаяся точка была ровно посередине толщины стены
                alignVector = bbCenterToProjectionVector.Normalize()
                    * (bbCenterToProjectionVector.GetLength() - wall.Width / 2);
            } else {
                alignVector = XYZ.Zero;
            }

            return bbCenter + alignVector;
        }
    }
}
