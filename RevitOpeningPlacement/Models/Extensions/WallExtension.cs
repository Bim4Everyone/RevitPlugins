using Autodesk.Revit.DB;

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
            var bb = wall.get_BoundingBox(null);
            return bb.Min + (bb.Max - bb.Min) / 2;
        }
    }
}
