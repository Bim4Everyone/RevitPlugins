using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class SolidExtension {
        public static Outline GetOutline(this Solid solid) {
            var bb = solid.GetBoundingBox();
            XYZ pt0 = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z);
            XYZ pt1 = new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z);
            XYZ pt2 = new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z);
            XYZ pt3 = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);

            var transform = bb.Transform;

            var tpt0 = transform.OfPoint(pt0);
            var tpt1 = transform.OfPoint(pt1);
            var tpt2 = transform.OfPoint(pt2);
            var tpt3 = transform.OfPoint(pt3);

            var tMax = transform.OfPoint(bb.Max);
            var points = new List<XYZ> { tpt0, tpt1, tpt2, tpt3 };

            var min = new XYZ(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z));
            var max = new XYZ(points.Max(p => p.X), points.Max(p => p.Y), tMax.Z);
            return new Outline(min, max);
        }

        public static bool IsNotClosed(this Solid solid) {
            return solid.Faces.IsEmpty || solid.Edges.IsEmpty;
        }

        public static Solid GetIntersection(this Solid solid1, Solid solid2) {
            return BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
        }

        public static Solid GetIntersection(this Solid solid1, Solid solid2, Transform solid2Transform) {
            return solid1.GetIntersection(SolidUtils.CreateTransformed(solid2, solid2Transform));
        }

        public static void DrowSolid(this Solid solid, Document doc, string name) {
            var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new[] { solid });
            ds.SetName(name);
        }
    }
}
