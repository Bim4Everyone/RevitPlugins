using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Константы и допуски, которые использует плагин
/// </summary>
internal static class ConstantsProvider {
    /// <summary>
    /// Точность для определения расстояний и координат: 1 мм в футах
    /// </summary>
    public const double ToleranceDistanceFeet = 1 / 304.8;

    /// <summary>
    /// Точность для определения объемов: 1 см3 в футах
    /// </summary>
    public const double ToleranceVolumeFeetCube = 10 / 304.8 * (10 / 304.8) * (10 / 304.8);

    /// <summary>
    /// Допуск процента объема в расчетах в долях от 1
    /// </summary>
    public const double ToleranceVolumePercentage = 0.01;

    /// <summary>
    /// Минимальное значение габарита задания на отверстие в футах (~5 мм)
    /// </summary>
    public const double OpeningTaskSizeMinValueFeet = 0.015;

    /// <summary>
    /// Значение округления координат в мм
    /// </summary>
    public const int CoordinateRoundMm = 5;

    /// <summary>
    /// Значение округления координат в единицах длины Revit (футах),
    /// равное конвертированному <see cref="CoordinateRoundMm"/>
    /// </summary>
    public static readonly double CoordinateRoundFeet =
        UnitUtils.ConvertToInternalUnits(CoordinateRoundMm, UnitTypeId.Millimeters);

    /// <summary>
    /// Допустимое расстояние между экземплярами семейств заданий на отверстия,
    /// при котором считается, что они размещены в одном и том же месте.
    /// Равно диагонали куба со стороной шага округления координат
    /// </summary>
    public static readonly double ToleranceDistance3dFeet =
        Math.Sqrt(3 * CoordinateRoundFeet * CoordinateRoundFeet);

    /// <summary>
    /// Допустимый объем, равный кубу <see cref="ToleranceDistance3dFeet"/>
    /// </summary>
    public static readonly double ToleranceVolume3dFeetCube =
        ToleranceDistance3dFeet * ToleranceDistance3dFeet * ToleranceDistance3dFeet;
}
