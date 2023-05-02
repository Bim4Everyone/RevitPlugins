using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class XYZExtension {
        /// <summary>
        /// Значение округления координат в мм
        /// </summary>
        private static readonly int _mmRound = 5;


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

        public static bool RunAlongWall(this XYZ direction, Wall wall) {
            var plane = wall.GetHorizontalNormalPlane();
            var wallLine = wall.GetLine();
            return plane.ProjectVector(direction).IsPapallel(plane.ProjectVector(wallLine.Direction));
        }

        /// <summary>
        /// Возвращает точку с округленными до 5 мм координатами
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XYZ Round(this XYZ value) {
            var x = RoundCoordinate(value.X);
            var y = RoundCoordinate(value.Y);
            var z = RoundCoordinate(value.Z);
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Округляет значение координаты точки на прямой до <see cref="_mmRound">заданного количества мм</see>
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private static double RoundCoordinate(double coordinate) {
#if REVIT_2020_OR_LESS
            double roundFeet = UnitUtils.ConvertToInternalUnits(_mmRound, DisplayUnitType.DUT_MILLIMETERS);
#else
            double roundFeet = UnitUtils.ConvertToInternalUnits(_mmRound, UnitTypeId.Millimeters);
#endif
            return Math.Round(coordinate / roundFeet) * roundFeet;
        }
    }
}
