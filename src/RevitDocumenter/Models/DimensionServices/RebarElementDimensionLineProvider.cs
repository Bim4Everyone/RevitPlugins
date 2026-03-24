using System;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.DimensionServices;
internal class RebarElementDimensionLineProvider : IDimensionLineProvider<RebarElementDimensionLineProviderContext> {
    /// <summary>
    /// Создает линию единичной длины перпендикулярно передаваемому направлению
    /// </summary>
    public Line GetPerpendicularLine(XYZ point, XYZ direction) {
        return Line.CreateBound(point, point + direction.CrossProduct(XYZ.BasisZ));
    }

    public Line GetDimensionLine(RebarElementDimensionLineProviderContext context) {
        return context.Rebar.Rebar.Location is not LocationPoint pt
            ? throw new ArgumentException(nameof(context.Rebar))
            : GetPerpendicularLine(pt.Point, context.Direction);
    }
}
