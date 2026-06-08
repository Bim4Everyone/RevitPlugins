using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal sealed class LoadArea {
    public LoadArea(Element element, IList<Polygon2D> circuits) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Circuits = circuits ?? throw new ArgumentNullException(nameof(circuits));
    }

    public Element Element { get; }

    public IList<Polygon2D> Circuits { get; }

    public double GetArea() {
        if(Circuits.Count == 0) {
            return 0;
        }
        double outer = Circuits[0].Area;
        for(int i = 1; i < Circuits.Count; i++) {
            outer -= Circuits[i].Area;
        }
        return Math.Max(0, outer);
    }
}
