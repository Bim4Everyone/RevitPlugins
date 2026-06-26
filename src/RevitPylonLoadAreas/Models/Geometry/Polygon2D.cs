using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal sealed class Polygon2D {
    /// <summary>
    /// Конструирует замкнутый многоугольник
    /// </summary>
    /// <param name="vertices">Список вершин многоугольника от первой до последней</param>
    public Polygon2D(IList<XY> vertices) {
        Vertices = vertices?.ToArray() ?? throw new ArgumentNullException(nameof(vertices));
        if(Vertices.Count < 3) {
            throw new ArgumentOutOfRangeException(nameof(vertices));
        }
    }

    public IReadOnlyList<XY> Vertices { get; }

    public CurveLoop AsCurveLoop() {
        var loop = new CurveLoop();
        int n = Vertices.Count;
        for(int i = 0; i < n; i++) {
            var a = Vertices[i].AsXYZ();
            var b = Vertices[(i + 1) % n].AsXYZ();
            loop.Append(Line.CreateBound(a, b));
        }

        return loop;
    }
}
