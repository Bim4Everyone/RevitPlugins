using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class LineExtention {
        public static bool IsHorizontal(this Line line) {
            return Math.Abs(line.GetEndPoint(0).Z - line.GetEndPoint(1).Z) < 0.0001;
        }

        public static bool IsPerpendicular(this Line line, XYZ direction) {
            return Math.Abs(line.Direction.AngleTo(direction)) < 0.0001
                || Math.Abs(line.Direction.AngleTo(direction) - Math.PI) < 0.0001;
        }

        public static XYZ GetIntersectionWithFace(this Line line, Face face) {
            face.Intersect(line, out IntersectionResultArray result);
            return result.get_Item(0).XYZPoint;
        }
    }
}
