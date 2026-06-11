using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal sealed class Polygon3D {
    /// <summary>
    /// Конструирует замкнутый многоугольник в пространстве
    /// </summary>
    /// <param name="vertices">Список вершин многоугольника от первой до последней</param>
    public Polygon3D(IList<XYZ> vertices) {
        Vertices = vertices?.ToArray() ?? throw new ArgumentNullException(nameof(vertices));
        if(Vertices.Count < 3) {
            throw new ArgumentOutOfRangeException(nameof(vertices));
        }
    }

    public IReadOnlyList<XYZ> Vertices { get; }

    public CurveLoop AsCurveLoop() {
        var loop = new CurveLoop();
        int n = Vertices.Count;
        for(int i = 0; i < n; i++) {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % n];
            loop.Append(Line.CreateBound(a, b));
        }

        return loop;
    }

    /// <summary>
    /// Проецирует многоугольник на плоскость XOY
    /// </summary>
    public Polygon2D ToPolygon2D() {
        return new Polygon2D(Vertices.Select(v => new XY(v)).ToArray());
    }
}
