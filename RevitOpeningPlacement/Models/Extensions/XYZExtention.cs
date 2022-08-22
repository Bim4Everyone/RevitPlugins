using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class XYZExtention {
        internal static bool IsPapallel(this XYZ vector1, XYZ vector2) {
            return Math.Abs(Math.Abs(Math.Cos(vector1.AngleTo(vector2))) - 1) < 0.0001;
        }

        public static bool IsPerpendicular(this XYZ vector1, XYZ vector2) {
            return Math.Abs(Math.Cos(vector1.AngleTo(vector2))) < 0.0001;
        }

        public static XYZ ProjectOnXoY(this XYZ xyz) {
            return new XYZ(xyz.X, xyz.Y, 0);
        }

        public static XYZ ProjectOnXoZ(this XYZ xyz) {
            return new XYZ(0, xyz.Y, xyz.Z);
        }

        public static XYZ ProjectOnYoZ(this XYZ xyz) {
            return new XYZ(xyz.X, 0, xyz.Z);
        }
    }
}
