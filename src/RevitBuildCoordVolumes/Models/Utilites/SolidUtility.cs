using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Utilites;

internal static class SolidUtility {
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
    /// <param name="start">Старт экструзии.</param>
    /// <param name="finish">Финиш экструзии.</param>
    /// <param name="up">Направление экструзии. По умолчанию вверх - True</param>
    /// <returns>
    /// Solid.
    /// </returns>
    public static Solid ExtrudeSolid(List<CurveLoop> listCurveLoops, double start, double finish, bool up = true) {
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
}
