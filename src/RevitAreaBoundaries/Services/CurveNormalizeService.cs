using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class CurveNormalizeService (SystemPluginConfig systemPluginConfig){
    private readonly double _tolerance = systemPluginConfig.DefaultTolerance;
    
    public List<Curve> ProjectCurvesToXy(List<Curve> curves) {
        return curves.Select(ProjectCurveToXy).ToList();
    }
    
    private Curve ProjectCurveToXy(Curve curve) {
        return curve switch {
            Line line => ProjectLine(line),
            Arc arc => (Curve)ProjectArc(arc) 
                       ?? Line.CreateBound(
                           ToXy(arc.GetEndPoint(0)), 
                           ToXy(arc.GetEndPoint(1))),
            _ => curve
        };
    }
    
    private static Line ProjectLine(Line line) {
        var p1 = ToXy(line.GetEndPoint(0));
        var p2 = ToXy(line.GetEndPoint(1));

        return Line.CreateBound(p1, p2);
    }
    
    private Arc ProjectArc(Arc arc) {
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);
        double tm = (t0 + t1) * 0.5;

        var start = ToXy(arc.Evaluate(t0, false));
        var mid   = ToXy(arc.Evaluate(tm, false));
        var end   = ToXy(arc.Evaluate(t1, false));

        double area = ((mid.X - start.X) * (end.Y - start.Y)) - ((mid.Y - start.Y) * (end.X - start.X));

        return Math.Abs(area) < _tolerance
            ? null 
            : Arc.Create(start, end, mid);
    }
    
    private static XYZ ToXy(XYZ p) {
        return new XYZ(p.X, p.Y, 0);
    }
}
