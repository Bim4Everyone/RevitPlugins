using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionLines;
internal class RebarZoneDimensionLineProvider : IDimensionLineProvider<RebarZoneDimensionLineProviderContext> {
    /// <summary>
    /// Создает линию единичной длины перпендикулярно передаваемому направлению
    /// </summary>
    public Line GetPerpendicularLine(XYZ point, XYZ direction) {
        point.ThrowIfNull();
        direction.ThrowIfNull();
        return Line.CreateBound(point, point + direction.CrossProduct(XYZ.BasisZ));
    }

    public Line GetDimensionLine(RebarZoneDimensionLineProviderContext context) {
        context.ThrowIfNull();
        context.Element.ThrowIfNull();
        context.Direction.ThrowIfNull();
        return context.Element.Location is not LocationPoint pt
            ? throw new ArgumentException(nameof(context.Element))
            : GetPerpendicularLine(pt.Point, context.Direction);
    }
}
