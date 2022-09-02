using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class BoundingBoxExtensions {
        public static BoundingBoxXYZ GetTransformedBoundingBox(this BoundingBoxXYZ bb) {
            return GetTransformedBoundingBox(bb, bb.Transform);
        }

        public static BoundingBoxXYZ GetTransformedBoundingBox(this BoundingBoxXYZ bb, Transform transform) {
            XYZ pt0 = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z);
            XYZ pt1 = new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z);
            XYZ pt2 = new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z);
            XYZ pt3 = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);

            var tpt0 = transform.OfPoint(pt0);
            var tpt1 = transform.OfPoint(pt1);
            var tpt2 = transform.OfPoint(pt2);
            var tpt3 = transform.OfPoint(pt3);

            var tMax = transform.OfPoint(bb.Max);
            var points = new List<XYZ> { tpt0, tpt1, tpt2, tpt3 };

            var min = new XYZ(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z));
            var max = new XYZ(points.Max(p => p.X), points.Max(p => p.Y), tMax.Z);
            return new BoundingBoxXYZ() { Min = min, Max = max };
        }

        public static BoundingBoxXYZ GetIntersection(this BoundingBoxXYZ bb1, BoundingBoxXYZ bb2) {
            return new BoundingBoxXYZ() {
                Min = new XYZ(Math.Max(bb1.Min.X, bb2.Min.X), Math.Max(bb1.Min.Y, bb2.Min.Y), Math.Max(bb1.Min.Z, bb2.Min.Z)),
                Max = new XYZ(Math.Min(bb1.Max.X, bb2.Max.X), Math.Min(bb1.Max.Y, bb2.Max.Y), Math.Min(bb1.Max.Z, bb2.Max.Z))
            };
        }

        public static BoundingBoxXYZ GetCommonBoundingBox(this IEnumerable<BoundingBoxXYZ> bbs) {
            if(bbs.Any()) {
                return new BoundingBoxXYZ() {
                    Min = new XYZ(bbs.Max(item => item.Min.X), bbs.Max(item => item.Min.Y), bbs.Max(item => item.Min.Z)),
                    Max = new XYZ(bbs.Min(item => item.Max.X), bbs.Min(item => item.Max.Y), bbs.Min(item => item.Max.Z)),
                };
            }
            return null;
        }
    }
}
