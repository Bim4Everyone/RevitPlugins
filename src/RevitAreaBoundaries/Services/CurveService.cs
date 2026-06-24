using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

public class CurveService {
    
    public List<CurvePiece> SplitToShortCurves(Curve curve, double maxLenCurveMm, int minSegments = 1) {
        if(curve == null) {
            return [];
        }
        if(minSegments <= 0) {
            minSegments = 1;
        }
        return curve switch {
            Line line => SplitLine(line, maxLenCurveMm, minSegments),
            Arc arc => SplitArc(arc, maxLenCurveMm, minSegments),
            _ => [new CurvePiece {
                SourceCurve = curve,
                Piece = curve
            }]
        };
    }

    private static List<CurvePiece> SplitLine(Line line, double maxSegmentLength, int minSegments) {
        double len = line.Length;
        if (len <= 1e-9) return [];

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        var p0 = line.GetEndPoint(0);
        var p1 = line.GetEndPoint(1);

        var res = new List<CurvePiece>(n);
        for (int i = 0; i < n; i++) {
            double a = i / (double)n;
            double b = (i + 1) / (double)n;

            var s = p0 + (p1 - p0) * a;
            var e = p0 + (p1 - p0) * b;

            if(!(s.DistanceTo(e) > 1e-9)) {
                continue;
            }

            var piece = Line.CreateBound(s, e);
            res.Add(new CurvePiece {
                SourceCurve = line,
                Piece = piece
            });
        }
        return res;
    }

    private static List<CurvePiece> SplitArc(Arc arc, double maxSegmentLength, int minSegments) {
        double len = arc.Length;
        
        if(len <= 1e-9) {
            return [];
        }

        int n = (int)Math.Ceiling(len / maxSegmentLength);
        n = Math.Max(n, minSegments);

        // Параметрический домен дуги
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);

        var res = new List<CurvePiece>(n);

        // Чтобы корректно создавать поддуги, берём три точки на каждой части:
        // start, mid, end. Тогда Arc.Create(start, end, mid) создаст нужную дугу.
        for (int i = 0; i < n; i++) {
            double a0 = i / (double)n;
            double a1 = (i + 1) / (double)n;

            double ta = t0 + (t1 - t0) * a0;
            double tb = t0 + (t1 - t0) * a1;
            double tm = (ta + tb) * 0.5;

            var pStart = arc.Evaluate(ta, false);
            var pMid   = arc.Evaluate(tm, false);
            var pEnd   = arc.Evaluate(tb, false);

            if(pStart.DistanceTo(pEnd) <= 1e-9) {
                continue;
            }
            
            // Создаём поддугу через 3 точки
            var subArc = Arc.Create(pStart, pEnd, pMid);
            res.Add(new CurvePiece {
                SourceCurve = arc,
                Piece = subArc
            });
        }

        return res;
    }
    
    public Curve ProjectCurveToXy(Curve curve) {
        return curve switch {
            Line line => ProjectLine(line),
            Arc arc => (Curve)ProjectArcSafe(arc) 
                       ?? Line.CreateBound(
                           ToXy(arc.GetEndPoint(0)), 
                           ToXy(arc.GetEndPoint(1))),
            _ => curve
        };
    }
    
    private static Arc ProjectArcSafe(Arc arc) {
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);
        double tm = (t0 + t1) * 0.5;

        var start = ToXy(arc.Evaluate(t0, false));
        var mid   = ToXy(arc.Evaluate(tm, false));
        var end   = ToXy(arc.Evaluate(t1, false));

        double area = ((mid.X - start.X) * (end.Y - start.Y)) - ((mid.Y - start.Y) * (end.X - start.X));

        if (Math.Abs(area) < 1e-9) {
            return null;
        }
        return Arc.Create(start, end, mid);
    }
    
    private static Line ProjectLine(Line line) {
        var p1 = ToXy(line.GetEndPoint(0));
        var p2 = ToXy(line.GetEndPoint(1));

        return Line.CreateBound(p1, p2);
    }
    
    private static XYZ ToXy(XYZ p) {
        return new XYZ(p.X, p.Y, 0);
    }
}
