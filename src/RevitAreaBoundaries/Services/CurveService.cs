using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

public class CurveService {
    
    public List<Curve> SplitToShortCurves(Curve curve, double maxLenCurveMm, int minSegments = 1) {
        if(curve == null) {
            return [];
        }
        if(minSegments <= 0) {
            minSegments = 1;
        }
        return curve switch {
            Line line => SplitLine(line, maxLenCurveMm, minSegments),
            Arc arc => SplitArc(arc, maxLenCurveMm, minSegments),
            _ => [curve]
        };
    }

    private static List<Curve> SplitLine(Line line, double maxSegmentLength, int minSegments)
    {
        double len = line.Length;
        if (len <= 1e-9) return new List<Curve>();

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        XYZ p0 = line.GetEndPoint(0);
        XYZ p1 = line.GetEndPoint(1);

        var res = new List<Curve>(n);
        for (int i = 0; i < n; i++)
        {
            double a = i / (double)n;
            double b = (i + 1) / (double)n;

            XYZ s = p0 + (p1 - p0) * a;
            XYZ e = p0 + (p1 - p0) * b;

            if (s.DistanceTo(e) > 1e-9)
                res.Add(Line.CreateBound(s, e));
        }
        return res;
    }

    private static List<Curve> SplitArc(Arc arc, double maxSegmentLength, int minSegments)
    {
        double len = arc.Length;
        if (len <= 1e-9) return new List<Curve>();

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        // Параметрический домен дуги
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);

        var res = new List<Curve>(n);

        // Чтобы корректно создавать поддуги, берём три точки на каждой части:
        // start, mid, end. Тогда Arc.Create(start, end, mid) создаст нужную дугу.
        for (int i = 0; i < n; i++)
        {
            double a0 = i / (double)n;
            double a1 = (i + 1) / (double)n;

            double ta = t0 + (t1 - t0) * a0;
            double tb = t0 + (t1 - t0) * a1;
            double tm = (ta + tb) * 0.5;

            XYZ pStart = arc.Evaluate(ta, false);
            XYZ pMid   = arc.Evaluate(tm, false);
            XYZ pEnd   = arc.Evaluate(tb, false);

            if (pStart.DistanceTo(pEnd) <= 1e-9)
                continue;

            // Создаём поддугу через 3 точки
            Arc subArc = Arc.Create(pStart, pEnd, pMid);
            res.Add(subArc);
        }

        return res;
    }
    
    public Curve ProjectCurveToXy(Curve curve) {
        var p1 = curve.GetEndPoint(0);
        var p2 = curve.GetEndPoint(1);
        return Line.CreateBound(
            new XYZ(p1.X, p1.Y, 0),
            new XYZ(p2.X, p2.Y, 0));
    }
    
}
