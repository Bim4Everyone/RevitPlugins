using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions;
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

    public static bool RunAlongWall(this XYZ direction, Wall wall) {
        var plane = wall.GetHorizontalNormalPlane();
        var wallLine = wall.GetLine();
        return plane.ProjectVector(direction).IsParallel(plane.ProjectVector(wallLine.Direction));
    }
}
