using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class XYZExtension {
        /// <summary>
        /// Значение округления координат в мм = 5 мм
        /// </summary>
        private static int MmRound => 5;

#if REVIT_2020_OR_LESS
        /// <summary>
        /// Значение округления координат в футах (единицах длины в Revit), равное конвертированному <see cref="MmRound"/>
        /// </summary>
        public static double FeetRound => UnitUtils.ConvertToInternalUnits(MmRound, DisplayUnitType.DUT_MILLIMETERS);
#else
        /// <summary>
        /// Значение округления координат в футах (единицах длины в Revit), равное конвертированному <see cref="MmRound"/>
        /// </summary>
        public static double FeetRound => UnitUtils.ConvertToInternalUnits(MmRound, UnitTypeId.Millimeters);
#endif


        internal static bool IsParallel(this XYZ vector1, XYZ vector2) {
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
            return plane.ProjectVector(direction).IsParallel(plane.ProjectVector(wallLine.Direction));
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
        /// Округляет значение координаты точки на прямой до <see cref="MmRound">заданного количества мм</see>
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private static double RoundCoordinate(double coordinate) {

            return Math.Round(coordinate / FeetRound) * FeetRound;
        }
    }
}
