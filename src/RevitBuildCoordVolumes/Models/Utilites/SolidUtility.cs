using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Utilites;
internal static class SolidUtility {
    // Направление экструзии - вверх
    private static readonly XYZ _directionUp = new(0, 0, 10);
    // Направление экструзии - вниз
    private static readonly XYZ _directionDown = new(0, 0, -10);

    public static Solid ExtrudeSolid(List<CurveLoop> listCurveLoops, double start, double finish, bool up = true) {
        if(start == double.NaN || finish == double.NaN) {
            return null;
        }

        var direction = up ? _directionUp : _directionDown;

        double amountToExtrude = finish - start;

        if(listCurveLoops.Count == 0 && amountToExtrude <= 1e-6) {
            return null;
        }

        try {
            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(listCurveLoops, direction, amountToExtrude);
            return solid != null && solid.Volume > 1e-6 ? solid : null;
        } catch {
            return null;
        }
    }
}
