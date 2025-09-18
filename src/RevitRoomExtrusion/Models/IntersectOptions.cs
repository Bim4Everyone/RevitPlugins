using Autodesk.Revit.DB;


namespace RevitRoomExtrusion.Models;
internal static class IntersectOptions {

    // Метод проверки идентичных (по ревиту лежащих в одной плоскости) линий в модели
    public static bool IsEqual(Curve curve1, Curve curve2) {
        return curve1.Intersect(curve2, out var results) == SetComparisonResult.Equal;
    }
}
