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
    
    private const double VolumeTolerance = 1e-9;

    /// <summary>
    /// Разбивает список солидов на непересекающиеся части.
    /// Каждый объем исходных солидов присутствует в результате ровно один раз.
    /// </summary>
    public static IList<Solid> Split(IList<Solid> solids) {
        var result = new List<Solid>();

        foreach (var sourceSolid in solids) {
            if (!IsValid(sourceSolid)) {
                continue;
            }

            var newSolid = sourceSolid;

            // Работаем со снимком текущего результата.
            // Внутри цикла result будет меняться.
            var existingParts = new List<Solid>(result);

            foreach (var existingPart in existingParts) {
                if (!IsValid(newSolid)) {
                    break;
                }

                var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    existingPart,
                    newSolid,
                    BooleanOperationsType.Intersect);

                if (!IsValid(intersection)) {
                    continue;
                }

                // Старая часть без пересечения.
                var existingRest = BooleanOperationsUtils.ExecuteBooleanOperation(
                    existingPart,
                    intersection,
                    BooleanOperationsType.Difference);

                // Новый Solid без пересечения.
                var newRest = BooleanOperationsUtils.ExecuteBooleanOperation(
                    newSolid,
                    intersection,
                    BooleanOperationsType.Difference);

                // Удаляем старую часть из result.
                result.Remove(existingPart);

                // Добавляем остаток старой части.
                if (IsValid(existingRest)) {
                    result.Add(existingRest);
                }

                // Добавляем общую часть.
                result.Add(intersection);

                // Продолжаем работать только с остатком нового Solid.
                newSolid = newRest;
            }

            // Всё, что осталось от нового Solid,
            // ещё ни с чем не пересеклось.
            if (IsValid(newSolid)) {
                result.Add(newSolid);
            }
        }

        return result;
    }

    private static bool IsValid(Solid solid) {
        return solid != null
               && solid.Volume > VolumeTolerance
               && solid.Faces.Size > 0;
    }
    


    public static Solid IntersectSolid(Solid solid1, Solid solid2) {
        try {
            var result = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            return result != null && result.Volume > GeometryTolerance.Model ? result : null;
        } catch {
            return null;
        }
    }

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
}
