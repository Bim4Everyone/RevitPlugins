using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

public class OuterSquareService(SystemPluginConfig systemPluginConfig) {
    private readonly double _margin = systemPluginConfig.DefaultCellsMargin;
    private readonly double _step = UnitUtils.ConvertToInternalUnits(
        systemPluginConfig.DefaultCellsCoarseStepMm, UnitTypeId.Millimeters);

    public List<XYZ> BuildOuterSquareVertices(IEnumerable<Curve> curves) {
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
        double margin = _margin * _step;

        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        return [new XYZ(minX, minY, 0), new XYZ(maxX, minY, 0), new XYZ(maxX, maxY, 0), new XYZ(minX, maxY, 0)];
    }
}
