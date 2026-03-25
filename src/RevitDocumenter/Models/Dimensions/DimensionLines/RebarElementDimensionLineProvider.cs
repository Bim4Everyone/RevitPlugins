using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal class RebarElementDimensionLineProvider : IDimensionLineProvider<RebarElementDimensionLineProviderContext> {
    /// <summary>
    /// Создает линию единичной длины перпендикулярно передаваемому направлению
    /// </summary>
    public Line GetPerpendicularLine(XYZ point, XYZ direction) {
        point.ThrowIfNull();
        direction.ThrowIfNull();
        return Line.CreateBound(point, point + direction.CrossProduct(XYZ.BasisZ));
    }

    public Line GetDimensionLine(RebarElementDimensionLineProviderContext context) {
        context.ThrowIfNull();
        context.Rebar.ThrowIfNull();
        context.Direction.ThrowIfNull();
        return context.Rebar.Rebar.Location is not LocationPoint pt
            ? throw new ArgumentException(nameof(context.Rebar))
            : GetPerpendicularLine(pt.Point, context.Direction);
    }
}
