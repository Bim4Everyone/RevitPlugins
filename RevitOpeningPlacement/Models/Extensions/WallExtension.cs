using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class WallExtension {
        public static Line GetСentralWallLine(this Wall wall) {
            var centralPoint = GetCentralPoint(wall);

            var wallLine = (Line) ((LocationCurve) wall.Location).Curve;

            var bottomCentralPoint = new XYZ(centralPoint.X, centralPoint.Y, wallLine.GetEndPoint(0).Z);

            return Line.CreateBound(bottomCentralPoint + wallLine.Direction * wallLine.Length / 2,
                                    bottomCentralPoint - wallLine.Direction * wallLine.Length / 2);
        }

        private static XYZ GetCentralPoint(Wall wall) {
            var bb = wall.get_BoundingBox(null);
            return bb.Min + (bb.Max - bb.Min) / 2;
        }
    }
}
