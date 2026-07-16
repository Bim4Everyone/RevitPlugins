using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class CurveNormalizeService (SystemPluginConfig systemPluginConfig, RevitRepository revitRepository){
    private readonly double _tolerance = systemPluginConfig.DefaultTolerance;
    
    public List<Curve> ProjectCurvesToXy(List<Curve> curves) {
        return curves.Select(ProjectCurveToXy)
            .Where(c => c != null)
            .ToList();
    }
    
    private Curve ProjectCurveToXy(Curve curve) {
        return curve switch {
            Line line => ProjectLine(line),
            Arc arc => ProjectArc(arc),
            _ => null
        };
    }
    
    private Curve ProjectLine(Line line) {
        double tolerance = revitRepository.Application.ShortCurveTolerance;
        var p1 = ToXy(line.GetEndPoint(0));
        var p2 = ToXy(line.GetEndPoint(1));

        return IsTooShort(p1, p2) 
            ? null 
            : Line.CreateBound(p1, p2);
    }
    
    private Curve ProjectArc(Arc arc) {
        double t0 = arc.GetEndParameter(0);
        double t1 = arc.GetEndParameter(1);
        double tm = (t0 + t1) * 0.5;

        var start = ToXy(arc.Evaluate(t0, false));
        var mid   = ToXy(arc.Evaluate(tm, false));
        var end   = ToXy(arc.Evaluate(t1, false));
        
        return IsTooShort(start, end) 
            ? null 
            : Arc.Create(start, end, mid);
    }
    
    private bool IsTooShort(XYZ p1, XYZ p2) {
        return p1.DistanceTo(p2) < revitRepository.Application.ShortCurveTolerance;
    }
    
    private static XYZ ToXy(XYZ p) {
        return new XYZ(p.X, p.Y, 0);
    }
}
