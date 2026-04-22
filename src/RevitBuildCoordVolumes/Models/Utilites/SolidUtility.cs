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

    /// <summary>
    /// Метод безопасного объединения списка солидов.
    /// </summary>
    /// <remarks>
    /// В данном методе производится безопасное объединение списка солидов в объединенный список солидов.
    /// </remarks>    
    /// <param name="solids">Исходный список солидов</param>
    /// <returns>
    /// Объединенный список солидов.
    /// </returns>
    public static IList<Solid> CreateUnitedSolids(IList<Solid> solids) {
        return SolidExtensions.CreateUnitedSolids([.. solids]);
    }
}
