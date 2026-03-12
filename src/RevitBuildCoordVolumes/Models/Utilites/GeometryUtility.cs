using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Utilites;
internal static class GeometryUtility {
    /// <summary>
    /// Метод проверки точки.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка, находится ли точка внутри полигона точек.
    /// </remarks>
    /// <param name="point">Искомая точка.</param>
    /// <param name="polygon">Список точек внутри которых ведется поиск (полигон точек).</param>
    /// <returns>
    /// True - точка внутри, False - точки внутри нет.
    /// </returns>
    public static bool IsPointInsidePolygon(XYZ point, List<XYZ> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            if((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                 (polygon[j].Y - polygon[i].Y) + polygon[i].X) {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>
    /// Метод сортировки кривых.
    /// </summary>
    /// <remarks>
    /// В данном методе производится сортировка кривых для дальнейшего построения замкнутого контура, 
    /// кривые при необходимости разворачиваются.
    /// </remarks>
    /// <param name="curves">Список исходных кривых для сортировки.</param>    
    /// <returns>
    /// Список отсортированных кривых.
    /// </returns>
    public static List<Curve> SortCurvesToLoop(List<Curve> curves) {
        if(curves.Count == 0) {
            return [];
        }

        // --- 2. Начинаем цикл с первой кривой ---
        var ordered = new List<Curve> { curves[0] };
        curves.RemoveAt(0);

        // --- 3. Строим цепочку ---
        while(curves.Count > 0) {
            var last = ordered[ordered.Count - 1];
            var lastEnd = last.GetEndPoint(1);

            bool found = false;

            for(int i = 0; i < curves.Count; i++) {
                var curve = curves[i];
                var start = curve.GetEndPoint(0);
                var end = curve.GetEndPoint(1);

                if(lastEnd.IsAlmostEqualTo(start)) {
                    ordered.Add(curve);
                    curves.RemoveAt(i);
                    found = true;
                    break;
                } else if(lastEnd.IsAlmostEqualTo(end)) {
                    // reversed curve
                    ordered.Add(curve.CreateReversed());
                    curves.RemoveAt(i);
                    found = true;
                    break;
                }
            }

            if(!found) {
                return [];
            }
        }

        return ordered;
    }
}
