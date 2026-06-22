using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Services;

public class OuterSquareService {
    
    public List<XYZ> BuildOuterSquareVertices(IEnumerable<Curve> curves, double step, int marginCells = 50) {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach(var curve in curves) {
            foreach(var point in curve.Tessellate()) {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        // запас вокруг модели
        double margin = marginCells * step;

        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        return [new XYZ(minX, minY, 0), new XYZ(maxX, minY, 0), new XYZ(maxX, maxY, 0), new XYZ(minX, maxY, 0)];
    }
}
