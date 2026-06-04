using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Drawing;

/// <summary>
/// Рисует ребра грузовых площадей на указанном виде detail-кривыми.
/// </summary>
internal sealed class DetailLineDrawer {
    private readonly Document _document;

    public DetailLineDrawer(Document document) {
        _document = document;
    }

    /// <summary>
    /// Рисует все ребра каждой грузовой площади на виде <paramref name="view"/>.
    /// Z-координата задается параметром <paramref name="elevation"/>.
    /// Каждое ребро становится отдельной detail-линией.
    /// Метод должен вызываться внутри существующей транзакции.
    /// </summary>
    public void Draw(View view, double elevation, IEnumerable<LoadArea> loadAreas) {
        foreach(var area in loadAreas) {
            foreach(var piece in area.Pieces) {
                DrawRing(view, elevation, piece.OuterRing);
                foreach(var hole in piece.Holes) {
                    DrawRing(view, elevation, hole);
                }
            }
        }
    }

    private void DrawRing(View view, double elevation, IReadOnlyList<Point2D> ring) {
        if(ring == null || ring.Count < 2) {
            return;
        }
        int n = ring.Count;
        for(int i = 0; i < n; i++) {
            var a = ring[i];
            var b = ring[(i + 1) % n];
            if(a.IsAlmostEqual(b)) {
                continue;
            }
            var p0 = new XYZ(a.X, a.Y, elevation);
            var p1 = new XYZ(b.X, b.Y, elevation);
            try {
                var line = Line.CreateBound(p0, p1);
                _document.Create.NewDetailCurve(view, line);
            } catch(Autodesk.Revit.Exceptions.ArgumentsInconsistentException) {
                // Очень короткое или вырожденное ребро после клиппинга — пропускаем.
            }
        }
    }
}
