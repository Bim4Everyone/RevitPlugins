using System;
using System.Collections.Generic;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal readonly struct DelaunayTriangle {
    public DelaunayTriangle(int v0, int v1, int v2, IReadOnlyList<XY> points) {
        V0 = v0;
        V1 = v1;
        V2 = v2;
        var a = points[v0];
        var b = points[v1];
        var c = points[v2];
        double ax = a.X, ay = a.Y;
        double bx = b.X, by = b.Y;
        double cx = c.X, cy = c.Y;
        double d = 2.0 * ((ax * (by - cy)) + (bx * (cy - ay)) + (cx * (ay - by)));
        XY center;
        if(Math.Abs(d) < GeometryTolerance.Model) {
            center = new XY((ax + bx + cx) / 3.0, (ay + by + cy) / 3.0);
        } else {
            double ux = (((ax * ax) + (ay * ay)) * (by - cy)
                         + ((bx * bx) + (by * by)) * (cy - ay)
                         + ((cx * cx) + (cy * cy)) * (ay - by))
                        / d;
            double uy = (((ax * ax) + (ay * ay)) * (cx - bx)
                         + ((bx * bx) + (by * by)) * (ax - cx)
                         + ((cx * cx) + (cy * cy)) * (bx - ax))
                        / d;
            center = new XY(ux, uy);
        }

        Circumcenter = center;
        double dxA = center.X - a.X;
        double dyA = center.Y - a.Y;
        CircumradiusSquared = dxA * dxA + dyA * dyA;
    }

    public int V0 { get; }
    public int V1 { get; }
    public int V2 { get; }
    public XY Circumcenter { get; }
    public double CircumradiusSquared { get; }

    public bool CircumcircleContains(XY p) {
        double dx = p.X - Circumcenter.X;
        double dy = p.Y - Circumcenter.Y;
        return dx * dx + dy * dy <= CircumradiusSquared + GeometryTolerance.Model;
    }
}
