using System;
using System.Collections.Generic;
using System.Linq;

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

        public static bool IntersectedWith(this BoundingBoxXYZ currentBb, BoundingBoxXYZ bb) {
            return !((currentBb.Min.X > bb.Max.X && currentBb.Min.Y > bb.Max.Y)
                    || (currentBb.Min.Z > bb.Max.Z && currentBb.Min.X > bb.Max.X)
                    || (currentBb.Min.Z > bb.Max.Z && currentBb.Min.Y > bb.Max.Y)
                    || (currentBb.Max.X < bb.Min.X && currentBb.Max.Y < bb.Min.Y)
                    || (currentBb.Max.Z < bb.Min.Z && currentBb.Max.X < bb.Min.X)
                    || (currentBb.Max.Z < bb.Min.Z && currentBb.Max.Y < bb.Min.Y));
        }

        public static bool IsIntersected(this IEnumerable<BoundingBoxXYZ> bbs) {
            var boundings = bbs.ToArray();
            if(boundings.Length == 0) { return false; }
            var currentBb = boundings[0];
            for(int i = 1; i < boundings.Length; i++) {
                if(!currentBb.IntersectedWith(boundings[i])) {
                    return false;
                }
                currentBb = currentBb.GetIntersection(boundings[i]);
            }
            return true;
        }

        public static BoundingBoxXYZ GetCommonBoundingBox(this IEnumerable<BoundingBoxXYZ> bbs) {
            if(bbs.IsIntersected()) {
                return bbs.GetIntersectedBb();
            }
            return bbs.GetUnitedBb();
        }

        private static BoundingBoxXYZ GetIntersectedBb(this IEnumerable<BoundingBoxXYZ> bbs) {
            if(bbs.Any()) {
                return new BoundingBoxXYZ() {
                    Min = new XYZ(bbs.Max(item => item.Min.X), bbs.Max(item => item.Min.Y), bbs.Max(item => item.Min.Z)),
                    Max = new XYZ(bbs.Min(item => item.Max.X), bbs.Min(item => item.Max.Y), bbs.Min(item => item.Max.Z)),
                };
            }
            return null;
        }

        private static BoundingBoxXYZ GetUnitedBb(this IEnumerable<BoundingBoxXYZ> bbs) {
            if(bbs.Any()) {
                return new BoundingBoxXYZ() {
                    Min = new XYZ(bbs.Min(item => item.Min.X), bbs.Min(item => item.Min.Y), bbs.Min(item => item.Min.Z)),
                    Max = new XYZ(bbs.Max(item => item.Max.X), bbs.Max(item => item.Max.Y), bbs.Max(item => item.Max.Z)),
                };
            }
            return null;
        }

    }
}
