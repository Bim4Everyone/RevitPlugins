using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

namespace RevitBuildCoordVolumes.Models.Utilites;

internal static class SolidUtility {
    // Старт для тестовой экструзии
    private const double _startDefault = 0;
    // Финиш для тестовой экструзии
    private const double _finishDefault = 1;
    // Направление экструзии - вверх
    private static readonly XYZ _directionUp = new(0, 0, 10);
    // Направление экструзии - вниз
    private static readonly XYZ _directionDown = new(0, 0, -10);

    /// <summary>
    /// Метод экструзии объемных элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится экструзия объемных элементов по заданным параметрам старта и финиша экструзии.
    /// </remarks>
    /// <param name="listCurveLoops">Замкнутый контур для экструзии.</param>
    /// <param name="start">Старт экструзии. По умолчанию 0.</param>
    /// <param name="finish">Финиш экструзии. По умолчанию 1.</param>
    /// <param name="up">Направление экструзии. По умолчанию вверх - True</param>
    /// <returns>
    /// Solid.
    /// </returns>
    public static Solid ExtrudeSolid(
        List<CurveLoop> listCurveLoops,
        double start = _startDefault,
        double finish = _finishDefault,
        bool up = true) {

        if(start == double.NaN || finish == double.NaN) {
            return null;
        }

        var direction = up ? _directionUp : _directionDown;

        double amountToExtrude = finish - start;

        if(listCurveLoops.Count == 0 && amountToExtrude <= GeometryTolerance.Model) {
            return null;
        }

        try {
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(listCurveLoops, direction, amountToExtrude);
            return solid != null && solid.Volume > GeometryTolerance.Model ? solid : null;
        } catch {
            return null;
        }
    }

    /// <summary>
    /// Метод перемешивания солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится перемешивание солидов по случайному GUID.
    /// </remarks>    
    /// <param name="solids">Исходный список солидов</param>
    /// <returns>
    /// Перемешанный список солидов.
    /// </returns>
    public static List<Solid> ShuffleSolidsByGuid(IEnumerable<Solid> solids) {
        return [.. solids.OrderBy(_ => Guid.NewGuid())];
    }

    /// <summary>
    /// Метод безопасного получения общего объема солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится вычисление общего объема солидов.
    /// </remarks>    
    /// <param name="solids">Исходный список солидов</param>
    /// <returns>
    /// Сумма объема всех солидов.
    /// </returns>
    public static double GetSolidsVolume(IList<Solid> solids) {
        return solids
            .Select(solid => (double) SolidExtensions.GetVolumeOrDefault(solid, 0))
            .Sum();
    }
    
    public static bool IsIntersect(Solid solid1, Solid solid2) {
        if (solid1 is null || solid2 is null)
            return false;
        try {
            var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                solid1,
                solid2,
                BooleanOperationsType.Intersect);

            return intersection is not null && GetSafeSolidVolume(intersection) > 0;
        }
        catch (Exception) {
            return false;
        }
    }
    
    // Метод безопасного получения объёма солида
    private static double GetSafeSolidVolume(Solid solid) {
        return solid?.Volume ?? 0;
    }
    
    public static List<Solid> GetSplitSolids(IList<Solid> solids) {
        return solids
            .SelectMany(SolidUtils.SplitVolumes)
            .ToList();
    }
}
